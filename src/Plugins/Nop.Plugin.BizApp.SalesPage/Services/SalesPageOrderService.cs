using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Services.Caching;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public class SalesPageOrderService
    {
        #region Fields

        private readonly IRepository<SalesPageOrderItem> _salesPageOrderItemRepository;
        private readonly IRepository<SalesPageOrder> _salesPageOrderRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly ICustomerService _customerService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly IRepository<Product> _productRepository;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ILanguageService _languageService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly SalesPageProductService _salesPageProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public SalesPageOrderService(
            IRepository<SalesPageOrderItem> salesPageOrderItemRepository,
            IRepository<SalesPageOrder> salesPageOrderRepository,
            IRepository<Order> orderRepository,
            IRepository<Address> addressRepository,
            ICustomerService customerService,
            BizAppHttpClient bizAppHttpClient,
            SalesPageRecordService salesPageRecordService,
            IAddressService addressService,
            IOrderService orderService,
            IRepository<Product> productRepository,
            ICustomNumberFormatter customNumberFormatter,
            IWorkContext workContext,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            ILanguageService languageService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IEventPublisher eventPublisher,
            SalesPageProductService salesPageProductService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            CatalogSettings catalogSettings,
            IDateTimeHelper dateTimeHelper)
        {
            _salesPageOrderItemRepository = salesPageOrderItemRepository;
            _salesPageOrderRepository = salesPageOrderRepository;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
            _customerService = customerService;
            _bizAppHttpClient = bizAppHttpClient;
            _salesPageRecordService = salesPageRecordService;
            _addressService = addressService;
            _orderService = orderService;
            _productRepository = productRepository;
            _customNumberFormatter = customNumberFormatter;
            _workContext = workContext;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _languageService = languageService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _urlRecordService = urlRecordService;
            _genericAttributeService = genericAttributeService;
            _eventPublisher = eventPublisher;
            _salesPageProductService = salesPageProductService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _catalogSettings = catalogSettings;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<IList<SalesPageOrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            return _salesPageOrderItemRepository.Table.Where(x => x.OrderId == orderId).ToList();
        }

        public async Task<SalesPageOrder> GetOrderByOrderIdAsync(int orderId)
        {
            return await _salesPageOrderRepository.Table.FirstAsync(x => x.OrderId == orderId);
        }

        public async Task<Order> PlaceOrderAsync(PlaceOrderRequest placeOrderRequest)
        {
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(placeOrderRequest.SalesPageRecordId);

            var salesPageRecordCustomer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);

            var products = await _productRepository.Table.ToListAsync();

            foreach (var cartItem in placeOrderRequest.CartItems)
            {
                var product = products.First(x => x.Sku == cartItem.ProductSku);
                if (product.StockQuantity != -100 && product.StockQuantity < cartItem.Quantity)
                    throw new NopException($"{product.Name} only has {product.StockQuantity} quantity left in stock");
            }

            //var postages = await _bizAppHttpClient.RequestAsync<GetPostageListRequest, PostageListResponse>(new GetPostageListRequest()
            //{
            //    Customer = salesPageRecordCustomer
            //});

            var shippingAddress = new Address()
            {
                Address1 = placeOrderRequest.Address1,
                Address2 = placeOrderRequest.Address2,
                City = placeOrderRequest.City,
                CountryId = placeOrderRequest.CountryId,
                CreatedOnUtc = DateTime.UtcNow,
                Email = placeOrderRequest.Email,
                FirstName = placeOrderRequest.FullName,
                PhoneNumber = placeOrderRequest.PhoneNumber,
                StateProvinceId = placeOrderRequest.StateProvinceId,
                ZipPostalCode = placeOrderRequest.ZipPostalCode
            };
            await _addressService.InsertAddressAsync(shippingAddress);

            var billingAddress = _addressService.CloneAddress(shippingAddress);
            await _addressService.InsertAddressAsync(billingAddress);

            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var customer = await _customerService.GetCustomerByIdAsync(placeOrderRequest.CustomerId);

            var store = await _storeContext.GetCurrentStoreAsync();

            var subTotal = placeOrderRequest.CartItems.Sum(x => x.Quantity * products.First(y => y.Sku == x.ProductSku).Price);
            var subTotalTax = subTotal * (decimal)0.08;

            //var postage = postages.PostageList.FirstOrDefault(x => x.Id == placeOrderRequest.Postage);
            var shippingTotal = 0;
            var shippingTotalTax = 0 * (decimal)0.08;

            var grandTotal = subTotal + subTotalTax + shippingTotal + shippingTotalTax;

            var order = new Order()
            {
                StoreId = store.Id,
                OrderGuid = Guid.NewGuid(),
                CustomerId = placeOrderRequest.CustomerId,
                CustomerTaxDisplayType = customer.TaxDisplayType ?? Nop.Core.Domain.Tax.TaxDisplayType.IncludingTax,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = subTotal + subTotalTax,
                OrderSubtotalExclTax = subTotal,
                OrderTax = subTotalTax,
                OrderShippingInclTax = shippingTotal + shippingTotalTax,
                OrderShippingExclTax = shippingTotal,
                OrderTotal = grandTotal,
                OrderStatus = OrderStatus.Processing,
                PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(salesPageRecord, SalesPageDefaults.PaymentTypeAttribute),
                PaymentStatus = PaymentStatus.Paid,
                PaidDateUtc = null,
                ShippingStatus = ShippingStatus.ShippingNotRequired,
                ShippingMethod = placeOrderRequest.Postage,
                ShippingRateComputationMethodSystemName = "Shipping.FixedByWeightByTotal",
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty,
                BillingAddressId = billingAddress.Id,
                ShippingAddressId = shippingAddress.Id,
            };

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : currentCurrency;
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            order.CustomerCurrencyCode = customerCurrency.CurrencyCode;
            order.CurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

            //customer language
            var customerLanguage = await _languageService.GetLanguageByIdAsync(customer.LanguageId ?? 0);
            if (customerLanguage == null || !customerLanguage.Published)
                customerLanguage = await _workContext.GetWorkingLanguageAsync();
            order.CustomerLanguageId = customerLanguage.Id;
            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            await _orderService.InsertOrderNoteAsync(new OrderNote()
            {
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
                Note = $"This order contains BizApp Products : {JsonConvert.SerializeObject(placeOrderRequest.CartItems)}"
            });

            //raise event       
            await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

            #region Sales Page Order

            var salesPageOrder = new SalesPageOrder()
            {
                AgentPId = placeOrderRequest.AgentPId,
                OrderId = order.Id,
                SalesPageRecordId = salesPageRecord.Id
            };
            await _salesPageOrderRepository.InsertAsync(salesPageOrder);

            var salesPageOrderItems = placeOrderRequest.CartItems
                .Select(x =>
                {
                    var product = products.First(y => y.Sku == x.ProductSku);
                    var salesPageOrderItem = new SalesPageOrderItem()
                    {
                        OrderId = order.Id,
                        Quantity = x.Quantity,
                        ProductSku = x.ProductSku,
                        Price = product.Price
                    };
                    salesPageOrderItem.TotalPrice = salesPageOrderItem.Price * salesPageOrderItem.Quantity;
                    salesPageOrderItem.TotalAgentPrice = salesPageOrderItem.AgentPrice * salesPageOrderItem.Quantity;

                    return salesPageOrderItem;
                })
                .ToList();
            await _salesPageOrderItemRepository.InsertAsync(salesPageOrderItems);

            salesPageOrder.TotalAgentPrice = salesPageOrderItems.Sum(x => x.TotalAgentPrice);
            await _salesPageOrderRepository.UpdateAsync(salesPageOrder);

            #endregion

            #region Call BizApp Api

            var orderData = new CreateOrderRequest.OrderData();

            orderData.Address = shippingAddress.Address1.TrimEnd(',');
            if (!string.IsNullOrEmpty(shippingAddress.Address2))
                orderData.Address += " " + shippingAddress.Address2.TrimEnd(',');
            orderData.Address += ", " + shippingAddress.City?.TrimEnd(',');
            orderData.Address += ", " + shippingAddress.ZipPostalCode?.TrimEnd(',');

            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddress.StateProvinceId.Value);
            orderData.Address += ", " + stateProvince.Name;

            var country = await _countryService.GetCountryByIdAsync(shippingAddress.CountryId.Value);
            orderData.Address += ", " + country.Name;

            orderData.PhoneNumber = shippingAddress.PhoneNumber;
            orderData.CustomerName = shippingAddress.FirstName;
            orderData.Email = shippingAddress.Email;
            orderData.PostageId = placeOrderRequest.Postage;

            orderData.PostagePrice = 0;

            orderData.OrderId = order.Id;
            //if (!string.IsNullOrEmpty(placeOrderRequest.AgentPId))
            //    orderData.TotalAgentPrice = placeOrderRequest.CartItems
            //        .Sum(x => x.Quantity * products.First(y => y.Sku == x.ProductSku)
            //    .AgentPrice);
            orderData.TotalPrice = order.OrderTotal;

            var paymentMethod = SalesPageDefaults.BizAppPay;

            var url = _webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord);
            if (!string.IsNullOrEmpty(placeOrderRequest.AgentPId))
                url += "/" + placeOrderRequest.AgentPId;

            var createOrderRequest = new CreateOrderRequest()
            {
                AgentPId = placeOrderRequest.AgentPId,
                Customer = salesPageRecordCustomer,
                SalesPageId = salesPageRecord.Id,
                Url = url,
                PaymentMethod = paymentMethod,
                ProductInfo = placeOrderRequest.CartItems.Select(x => new CreateOrderRequest.ProductData()
                {
                    ProductSku = x.ProductSku,
                    Quantity = x.Quantity
                }).ToList(),
                OrderInfo = new List<CreateOrderRequest.OrderData>()
                {
                    orderData
                }
            };

            //try
            //{
            //    var orderResponse = await _bizAppHttpClient.RequestAsync<CreateOrderRequest, OrderResponse>(createOrderRequest);

            //    if (orderResponse.Success)
            //    {
            //        await _orderService.InsertOrderNoteAsync(new OrderNote()
            //        {
            //            CreatedOnUtc = DateTime.UtcNow,
            //            OrderId = order.Id,
            //            Note = $"Successfully called BizApp CreateOrder Api with BizAppOrderId : {orderResponse.BizAppOrderId}"
            //        });

            //        salesPageOrder.BizAppOrderId = orderResponse.BizAppOrderId;
            //        salesPageOrder.FpxKey = orderResponse.FpxKey;
            //        salesPageOrder.TotalAgentPrice = orderResponse.TotalAgentPrice;
            //        salesPageOrder.AgentCommission = orderResponse.AgentCommission;
            //        await UpdateOrderAsync(salesPageOrder);

            //        order.OrderTotal = orderResponse.TotalPrice;
            //        await _orderService.UpdateOrderAsync(order);

            //        await _orderService.InsertOrderNoteAsync(new OrderNote()
            //        {
            //            CreatedOnUtc = DateTime.UtcNow,
            //            OrderId = order.Id,
            //            Note = $"Updated order total to {order.OrderTotal} and total agent price to {salesPageOrder.TotalAgentPrice} by api response"
            //        });
            //    }
            //    else
            //    {
            //        await _orderService.InsertOrderNoteAsync(new OrderNote()
            //        {
            //            CreatedOnUtc = DateTime.UtcNow,
            //            OrderId = order.Id,
            //            Note = $"Failed to called BizApp CreateOrder Api with response : {JsonConvert.SerializeObject(orderResponse)}"
            //        });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await _orderService.InsertOrderNoteAsync(new OrderNote()
            //    {
            //        CreatedOnUtc = DateTime.UtcNow,
            //        OrderId = order.Id,
            //        Note = $"Failed to called BizApp CreateOrder Api with request : {JsonConvert.SerializeObject(createOrderRequest)} and error message {ex.Message}"
            //    });
            //}

            #endregion

            return order;
        }

        public async Task UpdateOrderAsync(SalesPageOrder salesPageOrder)
        {
            await _salesPageOrderRepository.UpdateAsync(salesPageOrder);
        }

        public virtual async Task<IPagedList<Order>> SearchOrdersAsync(
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, List<int> psIds = null,
            string phone = null, string email = null, string customerName = "", string customOrderNumber = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = from o in _orderRepository.Table
                        join spo in _salesPageOrderRepository.Table on o.Id equals spo.OrderId
                        select o;

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            query = from o in query
                    join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                    where
                        (string.IsNullOrEmpty(phone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(phone))) &&
                        (string.IsNullOrEmpty(email) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(email))) &&
                        (string.IsNullOrEmpty(customerName) || (!string.IsNullOrEmpty(oba.FirstName) && oba.FirstName.Contains(customerName)))
                    select o;

            if (!string.IsNullOrEmpty(customOrderNumber))
                query = query.Where(o => o.CustomOrderNumber.Contains(customOrderNumber));

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        public virtual async Task<byte[]> ExportOrdersToXlsxAsync(IList<Order> orders)
        {
            //a vendor should have access only to part of order information
            var ignore = await _workContext.GetCurrentVendorAsync() != null;

            //lambda expressions for choosing correct order address
            async Task<Address> orderBillingAddress(Order o) => await _addressService.GetAddressByIdAsync(o.BillingAddressId);

            //property array
            var properties = new[]
            {
                new PropertyByName<Order, Language>("CustomOrderNumber", (p, l) => p.CustomOrderNumber),

                new PropertyByName<Order, Language>("PaymentStatus", (p, l) => p.PaymentStatusId, ignore)
                {
                    DropDownElements = await PaymentStatus.Pending.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Order, Language>("ShippingStatus", (p, l) => p.ShippingStatusId, ignore)
                {
                    DropDownElements = await ShippingStatus.ShippingNotRequired.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Order, Language>("Subtotal", (p, l) => p.OrderSubtotalExclTax, ignore),
                new PropertyByName<Order, Language>("Postage Fee", (p, l) => p.OrderShippingExclTax, ignore),
                new PropertyByName<Order, Language>("Order Total", (p, l) => p.OrderTotal, ignore),
                new PropertyByName<Order, Language>("Postage Id", (p, l) => p.ShippingMethod),
                new PropertyByName<Order, Language>("Posage Name", (p, l) => p.ShippingRateComputationMethodSystemName, ignore),
                new PropertyByName<Order, Language>("Created On", async (p, l) => await _dateTimeHelper.ConvertToUserTimeAsync(p.CreatedOnUtc, DateTimeKind.Utc)),
                new PropertyByName<Order, Language>("Customer Name", async (p, l) => (await orderBillingAddress(p))?.FirstName ?? string.Empty),
                new PropertyByName<Order, Language>("Email", async (p, l) => (await orderBillingAddress(p))?.Email ?? string.Empty),
                new PropertyByName<Order, Language>("PhoneNumber", async (p, l) => (await orderBillingAddress(p))?.PhoneNumber ?? string.Empty),
                new PropertyByName<Order, Language>("Address1", async (p, l) => (await orderBillingAddress(p))?.Address1 ?? string.Empty),
                new PropertyByName<Order, Language>("Address2", async (p, l) => (await orderBillingAddress(p))?.Address2 ?? string.Empty),
                new PropertyByName<Order, Language>("ZipPostalCode", async (p, l) => (await orderBillingAddress(p))?.ZipPostalCode ?? string.Empty),
                new PropertyByName<Order, Language>("State", async (p, l) => (await _stateProvinceService.GetStateProvinceByAddressAsync(await orderBillingAddress(p)))?.Name ?? string.Empty),
                new PropertyByName<Order, Language>("Country", async (p, l) => (await _countryService.GetCountryByAddressAsync(await orderBillingAddress(p)))?.Name ?? string.Empty),
            };

            //activity log
            await _customerActivityService.InsertActivityAsync("ExportOrders",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.ExportOrders"), orders.Count));

            return await ExportOrderToXlsxWithProductsAsync(properties, orders);
        }

        private async Task<byte[]> ExportOrderToXlsxWithProductsAsync(PropertyByName<Order, Language>[] properties, IEnumerable<Order> itemsToExport)
        {
            var orderItemProperties = new[]
            {
                new PropertyByName<SalesPageOrderItem, Language>("Sku",  (oi, l) => oi.ProductSku),
                new PropertyByName<SalesPageOrderItem, Language>("Price", (oi, l) => oi.Price),
                new PropertyByName<SalesPageOrderItem, Language>("Quantity", (oi, l) => oi.Quantity),
                new PropertyByName<SalesPageOrderItem, Language>("Agent Price", (oi, l) => oi.AgentPrice)
            };

            var orderItemsManager = new PropertyManager<SalesPageOrderItem, Language>(orderItemProperties, _catalogSettings);

            await using var stream = new MemoryStream();
            // ok, we can run the real code of the sample now
            using (var workbook = new XLWorkbook())
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handles to the worksheets
                // Worksheet names cannot be more than 31 characters
                var worksheet = workbook.Worksheets.Add(typeof(Order).Name);
                var fpWorksheet = workbook.Worksheets.Add("DataForProductsFilters");
                fpWorksheet.Visibility = XLWorksheetVisibility.VeryHidden;

                //create Headers and format them 
                var manager = new PropertyManager<Order, Language>(properties, _catalogSettings);
                manager.WriteDefaultCaption(worksheet);

                var row = 2;
                foreach (var order in itemsToExport)
                {
                    manager.CurrentObject = order;
                    await manager.WriteDefaultToXlsxAsync(worksheet, row++);

                    //a vendor should have access only to his products
                    var orderItems = await GetOrderItemsByOrderIdAsync(order.Id);

                    if (!orderItems.Any())
                        continue;

                    orderItemsManager.WriteDefaultCaption(worksheet, row, 2);
                    worksheet.Row(row).OutlineLevel = 1;
                    worksheet.Row(row).Collapse();

                    foreach (var orderItem in orderItems)
                    {
                        row++;
                        orderItemsManager.CurrentObject = orderItem;
                        await orderItemsManager.WriteDefaultToXlsxAsync(worksheet, row, 2, fpWorksheet);
                        worksheet.Row(row).OutlineLevel = 1;
                        worksheet.Row(row).Collapse();
                    }

                    row++;
                }

                workbook.SaveAs(stream);
            }

            return stream.ToArray();
        }

        #endregion
    }
}