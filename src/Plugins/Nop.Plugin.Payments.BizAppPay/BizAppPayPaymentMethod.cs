using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.BizApp.Core.Domain.Api.Auth;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.Payments.BizAppPay.Components;
using Nop.Plugin.Payments.BizAppPay.Domain;
using Nop.Plugin.Payments.BizAppPay.Domain.Api;
using Nop.Plugin.Payments.BizAppPay.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;

namespace Nop.Plugin.Payments.BizAppPay
{
    /// <summary>
    /// BizAppPay payment processor
    /// </summary>
    public class BizAppPayPaymentMethod : BasePlugin, IPaymentMethod
    {
        #region Constants

        /// <summary>
        /// Requery Url
        /// </summary>
        private const string REQUERY_URL = "/api/services/app/PaymentGateway/Query";

        /// <summary>
        /// Redirect Url. Url to return payment redirect
        /// </summary>
        private const string REDIRECT_URL = "{0}/Plugins/BizAppPay/PaymentComplete/{1}";

        /// <summary>
        /// Backend Url. Url to return payment response data
        /// </summary>
        private const string BACKEND_URL = "{0}/Plugins/BizAppPay/Webhook/{1}";

        #endregion

        #region Fields

        private readonly BizAppPayTransactionService _bizAppPayTransactionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly BizAppPayPaymentSettings _bizAppPayPaymentSettings;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly BizAppPayHttpClient _bizAppPayHttpClient;
        private readonly IPriceFormatter _priceFormatter;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly SalesPageOrderService _salesPageOrderService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public BizAppPayPaymentMethod(BizAppPayTransactionService bizAppPayTransactionService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            BizAppPayPaymentSettings bizAppPayPaymentSettings,
            ILogger logger,
            IPaymentPluginManager paymentPluginManager,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            IStoreService storeService,
            IPermissionService permissionService,
            BizAppPayHttpClient bizAppPayHttpClient,
            IPriceFormatter priceFormatter,
            BizAppHttpClient bizAppHttpClient,
            SalesPageOrderService salesPageOrderService,
            SalesPageRecordService salesPageRecordService,
            IUrlRecordService urlRecordService,
            ICustomerService customerService,
            IAddressService addressService)
        {
            _bizAppPayTransactionService = bizAppPayTransactionService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _bizAppPayPaymentSettings = bizAppPayPaymentSettings;
            _logger = logger;
            _paymentPluginManager = paymentPluginManager;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _storeService = storeService;
            _permissionService = permissionService;
            _bizAppPayHttpClient = bizAppPayHttpClient;
            _priceFormatter = priceFormatter;
            _bizAppHttpClient = bizAppHttpClient;
            _salesPageOrderService = salesPageOrderService;
            _salesPageRecordService = salesPageRecordService;
            _urlRecordService = urlRecordService;
            _customerService = customerService;
            _addressService = addressService;
        }

        #endregion

        #region Utilities

        public async Task<string> GetPaymentUrlAsync(string description, Order order)
        {
            var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var fullName = billingAddress.FirstName;
            if (!string.IsNullOrEmpty(billingAddress.LastName))
                fullName += " " + billingAddress.LastName;

            var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);
            var userInfoResponse = await _bizAppHttpClient.RequestAsync<GetUserInfoRequest, UserInfoResponse>(new GetUserInfoRequest()
            {
                Customer = customer
            });

            var createBillRequest = new CreateBillRequest()
            {
                CallbackUrl = await GetBackendUrlAsync(_webHelper.GetStoreHost(true)?.Trim('/'), order.CustomOrderNumber),
                WebReturnUrl = await GetRedirectUrlAsync(_webHelper.GetStoreHost(true)?.Trim('/'), order.CustomOrderNumber),
                Amount = order.OrderTotal.ToString("0.00"),
                ApiKey = userInfoResponse.UserInfo.First().BizAppPaySecretKey,
                Category = userInfoResponse.UserInfo.First().BizAppPayCategoryCode,
                Name = description + " #" + order.CustomOrderNumber + " #" + salesPageOrder.BizAppOrderId,
                PayerEmail = billingAddress.Email,
                PayerName = fullName,
                PayerPhone = billingAddress.PhoneNumber,
                ReferenceNumber = order.CustomOrderNumber
            };

            var collectionId = userInfoResponse.UserInfo.First().BizAppPayCategoryCode;
            if (!string.IsNullOrEmpty(salesPageOrder.AgentPId))
            {
                var validateSalesPageResponse = await _bizAppHttpClient.RequestAsync<BizApp.SalesPage.Domain.Api.ValidateSalesPageRequest, BizApp.SalesPage.Domain.Api.ValidateSalesPageResponse>
                    (new BizApp.SalesPage.Domain.Api.ValidateSalesPageRequest()
                    {
                        AgentPId = salesPageOrder.AgentPId,
                        Customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId),
                        SalesPageId = salesPageRecord.Id,
                        Url = _webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord)
                    });

                createBillRequest.SplitArgs = $"{{\"type\":2,\"split\":{{\"{validateSalesPageResponse.AgentBankInfo.First().BizAppPaySecretKey}\":{salesPageOrder.AgentCommission}}}}}";

                collectionId = validateSalesPageResponse.AgentBankInfo.First().BizAppPayCategoryCode;
            }

            var billResponse = await _bizAppPayHttpClient.RequestAsync<CreateBillRequest, CreateBillResponse>(createBillRequest);

            //insert transaction for tracking
            await _bizAppPayTransactionService.InsertBizAppPayTransactionAsync(new BizAppPayTransaction()
            {
                CustomOrderNumber = order.CustomOrderNumber,
                TransactionNumber = billResponse.BillCode
            });

            salesPageOrder.PaymentUrl = billResponse.Url;
            salesPageOrder.BillId = billResponse.BillCode;
            salesPageOrder.CollectionId = collectionId;
            await _salesPageOrderService.UpdateOrderAsync(salesPageOrder);

            return billResponse.Url;
        }

        /// <summary>
        /// Gets Redirect Url
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetRedirectUrlAsync(string baseUrl, string customOrderNumber)
        {
            return await Task.FromResult(string.Format(REDIRECT_URL, baseUrl, customOrderNumber));
        }

        /// <summary>
        /// Gets Backend Url
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetBackendUrlAsync(string baseUrl, string customOrderNumber)
        {
            return await Task.FromResult(string.Format(BACKEND_URL, baseUrl, customOrderNumber));
        }

        public async Task<string> ProcessCallbackAsync(string customOrderNumber)
        {
            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync(BizAppPayPaymentDefaults.SYSTEM_NAME) as BizAppPayPaymentMethod;
            if (processor == null ||
                !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("BizAppPay module cannot be loaded");

            var order = await _orderService.GetOrderByCustomOrderNumberAsync(customOrderNumber);

            #region Validate Values

            if (order == null)
            {
                await _logger.ErrorAsync("BizAppPay. Order is not found", new NopException(customOrderNumber));

                return string.Empty;
            }

            #endregion

            var bizAppPayTransaction = await _bizAppPayTransactionService.GetBizAppPayOrderByCustomOrderNumberAsync(customOrderNumber);

            var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);

            var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);
            var userInfoResponse = await _bizAppHttpClient.RequestAsync<GetUserInfoRequest, UserInfoResponse>(new GetUserInfoRequest()
            {
                Customer = customer
            });

            var billInfoResponse = await _bizAppPayHttpClient.RequestAsync<GetBillInfoByBillCodeRequest, BillInfoResponse>(new GetBillInfoByBillCodeRequest()
            {
                SearchString = bizAppPayTransaction.TransactionNumber,
                ApiKey = userInfoResponse.UserInfo.First().BizAppPaySecretKey
            });

            if (billInfoResponse.Bill.Payments.Where(x => x.Status != "1").Count() > 0)
            {
                //not valid
                var errorStr = string.Format("ReQuery transaction status : {0}. Order# {1}.", JsonConvert.SerializeObject(billInfoResponse),
                    customOrderNumber);
                //log
                await _logger.ErrorAsync(errorStr);

                //order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                });
                await _orderService.UpdateOrderAsync(order);

                return string.Empty;
            }

            //validate order total
            var billPaidAmount = billInfoResponse.Bill.Payments.Sum(x => x.PaidAmount);
            //if (!billPaidAmount.Equals(order.OrderTotal))
            //{
            //    //not valid
            //    var errorStr = string.Format("BizAppPay. Returned order total {0} doesn't equal order total {1}. Order# {2}.", billPaidAmount, order.OrderTotal, customOrderNumber);
            //    //log
            //    await _logger.ErrorAsync(errorStr);

            //    //order note
            //    await _orderService.InsertOrderNoteAsync(new OrderNote
            //    {
            //        Note = errorStr,
            //        DisplayToCustomer = false,
            //        CreatedOnUtc = DateTime.UtcNow,
            //        OrderId = order.Id
            //    });
            //    await _orderService.UpdateOrderAsync(order);

            //    return string.Empty;
            //}

            #region Standard payment

            try
            {
                //valid
                //order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    Note = JsonConvert.SerializeObject(billInfoResponse),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                });
                await _orderService.UpdateOrderAsync(order);

                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    order.AuthorizationTransactionId = billInfoResponse.Bill.BillCode;
                    await _orderService.UpdateOrderAsync(order);

                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(string.Format("BizAppPay for order number {0}", order.CustomOrderNumber), ex);
            }

            #endregion

            return customOrderNumber;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult()
            {
                NewPaymentStatus = PaymentStatus.Paid
            });
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var urlFormat = $"{_webHelper.GetStoreHost(true)?.Trim('/')}/Plugins/BizAppPay/PaymentRequest/{{0}}";
            _httpContextAccessor.HttpContext.Response.Redirect(string.Format(urlFormat,
                postProcessPaymentRequest.Order.CustomOrderNumber));
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(decimal.Zero);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="bizAppPayturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the bizAppPayture payment result
        /// </returns>
        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest bizAppPayturePaymentRequest)
        {
            return await Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return await Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return await Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return await Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreHost(true)?.Trim('/')}/Admin/PaymentBizAppPay/Configure";
        }

        /// <summary>
        /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component type</returns>
        public Type GetPublicViewComponent()
        {
            return typeof(BizAppPayPaymentComponent);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new BizAppPayPaymentSettings();
            await _settingService.SaveSettingAsync(settings);

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.BizAppPay.Fields.Endpoint"] = "Endpoint",
                ["Plugins.Payments.BizAppPay.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.BizAppPay.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.BizAppPay.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.BizAppPay.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.BizAppPay.RoundingWarning"] = "It looks like you have \"ShoppingCartSettings.RoundPricesDuringCalculation\" setting disabled. Keep in mind that this can lead to a discrepancy of the order total amount, as BizAppPayConverter only rounds to two decimals.",
                ["Plugins.Payments.BizAppPay.Fields.RedirectionTip"] = "You will be redirected to BizAppPay site for payment after order is completed.",
                ["Plugins.Payments.BizAppPay.ProductDescription"] = "Payment for BizApp"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<BizAppPayPaymentSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.BizAppPay");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.BizAppPay.PaymentMethodDescription");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether bizAppPayture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        #endregion
    }
}
