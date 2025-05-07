using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.BizApp.SalesPage.Factories
{
    public partial class SalesPageOrderModelFactory
    {
        #region Fields

        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly SalesPageOrderService _salesPageOrderService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IUrlRecordService _urlRecordService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerService _customerService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IAddressService _addressService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly SalesPageProductService _salesPageProductService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly Nop.Web.Factories.IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IRepository<Product> _productRepository;

        #endregion

        #region Ctor

        public SalesPageOrderModelFactory(
            SalesPageRecordService salesPageRecordService,
            SalesPageOrderService salesPageOrderService,
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IWebHelper webHelper,
            IUrlRecordService urlRecordService,
            BizAppHttpClient bizAppHttpClient,
            ICustomerService customerService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IAddressService addressService,
            IPriceFormatter priceFormatter,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            SalesPageProductService salesPageProductService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            Nop.Web.Factories.IAddressModelFactory addressModelFactory,
            AddressSettings addressSettings,
            ICurrencyService currencyService,
            IProductService productService,
            IPictureService pictureService,
            IRepository<Product> productRepository)
        {
            _salesPageRecordService = salesPageRecordService;
            _salesPageOrderService = salesPageOrderService;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _workContext = workContext;
            _webHelper = webHelper;
            _urlRecordService = urlRecordService;
            _bizAppHttpClient = bizAppHttpClient;
            _customerService = customerService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _addressService = addressService;
            _priceFormatter = priceFormatter;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _salesPageProductService = salesPageProductService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _addressModelFactory = addressModelFactory;
            _addressSettings = addressSettings;
            _currencyService = currencyService;
            _productService = productService;
            _pictureService = pictureService;
            _productRepository = productRepository;
        }

        #endregion

        #region Utilities

        private IList<int> GetAllowedStateIds(IList<StateProvince> stateProvinces, Country malaysiaCountry, PostageListResponse.PostageData postageData)
        {
            var result = new List<int>();

            var sabahStateName = "sabah";
            var sarawakStateName = "sarawak";

            if (postageData.ZoneSm == 1)
            {
                result.AddRange(stateProvinces.Where(x => x.CountryId == malaysiaCountry.Id
                    && x.Name.ToLower() != sarawakStateName && x.Name.ToLower() != sabahStateName)
                    .Select(x => x.Id));
            }

            if (postageData.ZoneSrwk == 1)
            {
                result.Add(stateProvinces.First(x => x.CountryId == malaysiaCountry.Id
                    && x.Name.ToLower() == sarawakStateName).Id);
            }

            if (postageData.ZoneSs == 1)
            {
                result.Add(stateProvinces.First(x => x.CountryId == malaysiaCountry.Id
                    && x.Name.ToLower() == sabahStateName).Id);
            }

            if (postageData.ZoneOthers == 1)
            {
                result.AddRange(stateProvinces.Where(x => x.CountryId != malaysiaCountry.Id)
                    .Select(x => x.Id));
            }

            return result;
        }

        #endregion

        #region Methods

        public virtual async Task<SalesPageOrderModel> PrepareSalesPageOrderModelAsync(SalesPageOrderModel model, SalesPageRecord salesPageRecord, bool excludeProperties = false)
        {
            var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);
            //var products = await _salesPageProductService.GetAllProductsAsync(customer, model.AgentPId);

            var products = await _productService.SearchProductsAsync();
            var salesPageProductSkus = JsonConvert.DeserializeObject<IList<string>>(await _genericAttributeService.GetAttributeAsync<string>(salesPageRecord, SalesPageDefaults.ProductSkusAttribute));
            model.AvailableProducts = await products
                .Where(x => (x.StockQuantity > 0 || x.StockQuantity == -100) && salesPageProductSkus.Contains(x.Sku))
                .SelectAwait(async x => new ProductModel()
                {
                    ProductId = x.Id,
                    ProductName = x.Name,
                    ProductSku = x.Sku,
                    Price = x.Price,
                    Weight = 0,
                    PriceStr = await _priceFormatter.FormatPriceAsync(x.Price),
                    Attachments = new List<string>(),
                    StockQuantity = x.StockQuantity,
                }).ToListAsync();

            foreach(var product in model.AvailableProducts)
            {
                var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.ProductId, 1)).FirstOrDefault();
                var (pictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                product.Attachments.Add(pictureThumbnailUrl);
            }

            //var postages = await _bizAppHttpClient.RequestAsync<GetPostageListRequest, PostageListResponse>(new GetPostageListRequest()
            //{
            //    Customer = customer
            //});
            var states = await _stateProvinceService.GetStateProvincesAsync();
            var malaysiaCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(SalesPageDefaults.MalaysiaTwoLetterIsoCode);
            model.AvailablePostages = new List<PostageModel>();

            if (salesPageRecord != null)
            {
                //fill in model values from the entity
                model ??= new SalesPageOrderModel();

                model.SalesPageRecordId = salesPageRecord.Id;
                model.PageName = salesPageRecord.PageName;
                model.PageHtmlContent = salesPageRecord.PageHtmlContent;
                model.UrlSlug = await _urlRecordService.GetSeNameAsync(salesPageRecord);

                if (!string.IsNullOrEmpty(model.AgentPId))
                    model.UrlSlug += $"/{model.AgentPId}";

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                }
            }

            //prepare available countries
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);

            //filter to only Malaysia 
            model.AvailableCountries = model.AvailableCountries.Where(country => country.Value == malaysiaCountry.Id.ToString()).ToList();
            model.CountryId = malaysiaCountry.Id;

            //prepare available states
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId,
                defaultItemText: await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PleaseSelect"));

            return model;
        }

        public virtual async Task<OrderSummaryModel> PrepareOrderSummaryModelAsync(Order order)
        {
            var model = new OrderSummaryModel()
            {
                Id = order.Id,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                CustomOrderNumber = order.CustomOrderNumber
            };

            var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

            await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress,
                address: shippingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
            model.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
            model.OrderShipping = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, languageId, true);

            model.SubtotalPrice = await _priceFormatter.FormatPriceAsync(order.OrderSubtotalExclTax, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId);
            model.Tax = await _priceFormatter.FormatPriceAsync(order.OrderTax, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId);
            model.Shipping = await _priceFormatter.FormatPriceAsync(order.OrderShippingExclTax, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId);
            model.ShippingTax = await _priceFormatter.FormatPriceAsync(order.OrderShippingInclTax - order.OrderShippingExclTax, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId);

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId);

            var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
            model.BizAppOrderId = salesPageOrder.BizAppOrderId;

            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);

            var seName = await _urlRecordService.GetSeNameAsync(salesPageRecord);
            model.SalesPageUrl = _webHelper.GetStoreLocation() + seName;
            if (!string.IsNullOrEmpty(salesPageOrder.AgentPId))
                model.SalesPageUrl += $"/{salesPageOrder.AgentPId}";

            var salesPageOwner = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);

            var orderItems = await _salesPageOrderService.GetOrderItemsByOrderIdAsync(order.Id);
            var products = await _productRepository.Table.ToListAsync();

            model.Products = await orderItems.SelectAwait(async x => new OrderSummaryModel.OrderItemModel()
            {
                Price = await _priceFormatter.FormatPriceAsync(x.Price, true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId),
                ProductSku = x.ProductSku,
                Quantity = x.Quantity,
                TotalPrice = await _priceFormatter.FormatPriceAsync((x.Price * x.Quantity * (decimal)1.08) + 0 + (0 * (decimal)0.08), true, (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode, false, languageId),
                ProductName = products.FirstOrDefault(y => y.Sku == x.ProductSku)?.Name ?? x.ProductSku
            }).ToListAsync();

            return model;


        }

        #endregion
    }
}
