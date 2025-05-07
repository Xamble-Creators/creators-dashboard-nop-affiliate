using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.Payments.BizAppPay.Domain.Api;
using Nop.Plugin.Payments.BizAppPay.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.BizAppPay.Controllers
{
    public class PaymentBizAppPayController : BasePaymentController
    {
        #region Constants

        #endregion

        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly SalesPageOrderService _salesPageOrderService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public PaymentBizAppPayController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentPluginManager paymentPluginManager,
            IOrderService orderService,
            ILocalizationService localizationService,
            ILogger logger,
            ShoppingCartSettings shoppingCartSettings,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            IAddressService addressService,
            ICustomerService customerService,
            IVendorService vendorService,
            IProductService productService,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            SalesPageOrderService salesPageOrderService,
            SalesPageRecordService salesPageRecordService,
            IUrlRecordService urlRecordService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _paymentPluginManager = paymentPluginManager;
            _orderService = orderService;
            _localizationService = localizationService;
            _logger = logger;
            _shoppingCartSettings = shoppingCartSettings;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _addressService = addressService;
            _customerService = customerService;
            _vendorService = vendorService;
            _productService = productService;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _salesPageOrderService = salesPageOrderService;
            _salesPageRecordService = salesPageRecordService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        #endregion

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bizAppPayPaymentSettings = await _settingService.LoadSettingAsync<BizAppPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.Endpoint = bizAppPayPaymentSettings.Endpoint;
            model.AdditionalFee = bizAppPayPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = bizAppPayPaymentSettings.AdditionalFeePercentage;
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Endpoint_OverrideForStore = await _settingService.SettingExistsAsync(bizAppPayPaymentSettings, x => x.Endpoint, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(bizAppPayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(bizAppPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.BizAppPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bizAppPayPaymentSettings = await _settingService.LoadSettingAsync<BizAppPayPaymentSettings>(storeScope);

            //save settings
            bizAppPayPaymentSettings.Endpoint = model.Endpoint;
            bizAppPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            bizAppPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(bizAppPayPaymentSettings, x => x.Endpoint, model.Endpoint_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bizAppPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bizAppPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> RoundingWarning()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (!_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.BizAppPay.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        public async Task<IActionResult> PaymentRequest(string customOrderNumber)
        {
            //var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync(BizAppPayPaymentDefaults.SYSTEM_NAME) as BizAppPayPaymentMethod;
            //if (processor == null ||
            //    !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
            //    throw new NopException("BizAppPay module cannot be loaded");

            var order = await _orderService.GetOrderByCustomOrderNumberAsync(customOrderNumber);

            try
            {
                //if payment already paid, redirect to order

                if (order.PaymentStatus == PaymentStatus.Paid)
                    return RedirectToRoute("SalesPageThankYou");

                var amount = order.OrderTotal;
                if (amount > 0)
                {
                    var paymentUrl = "";
                        //await processor
                        //.GetPaymentUrlAsync(await _localizationService.GetResourceAsync("Plugins.Payments.BizAppPay.ProductDescription"), order);
                    return Redirect(paymentUrl);
                }
                else
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = customOrderNumber;
                        await _orderService.UpdateOrderAsync(order);

                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    }

                    return RedirectToRoute("SalesPageThankYou");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Failed to get payment url", ex);

                var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
                if (salesPageOrder == null)
                    return NotFound();

                var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);
                if (salesPageRecord == null)
                    return NotFound();

                return new RedirectResult(_webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord)
                    + (!string.IsNullOrEmpty(salesPageOrder.AgentPId) ? "/" + salesPageOrder.AgentPId : string.Empty)
                    + "?msg=payment.failed");
            }
        }

        public async Task<IActionResult> PaymentComplete(string customOrderNumber)
        {
            await _logger.InformationAsync(string.Format("BizAppPay Redirect. ReferenceCode {0}", customOrderNumber));

            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync(BizAppPayPaymentDefaults.SYSTEM_NAME) as BizAppPayPaymentMethod;
            if (processor == null ||
                !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("BizAppPay module cannot be loaded");

            await processor.ProcessCallbackAsync(customOrderNumber);

            var order = await _orderService.GetOrderByCustomOrderNumberAsync(customOrderNumber);

            if (order.PaymentStatus == PaymentStatus.Paid)
                return RedirectToRoute("SalesPageThankYou", new { customOrderNumber = customOrderNumber });

            var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
            if (salesPageOrder == null)
                return NotFound();

            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);
            if (salesPageRecord == null)
                return NotFound();

            return new RedirectResult(_webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord)
                    + (!string.IsNullOrEmpty(salesPageOrder.AgentPId) ? "/" + salesPageOrder.AgentPId : string.Empty)
                    + "?msg=payment.failed");
        }

        [HttpPost]
        public async Task<IActionResult> Webhook(string customOrderNumber, [FromQuery] PaymentCallback paymentCallback)
        {
            await _logger.InformationAsync(string.Format("BizAppPay Callback. Request {0}", JsonConvert.SerializeObject(paymentCallback)));

            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync(BizAppPayPaymentDefaults.SYSTEM_NAME) as BizAppPayPaymentMethod;
            if (processor == null ||
                !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("BizAppPay module cannot be loaded");

            await processor.ProcessCallbackAsync(customOrderNumber);

            return Json("RECEIVEOK");
        }

        public async Task<IActionResult> CancelOrder()
        {
            var order = (await _orderService.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1)).FirstOrDefault();


            var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(order.Id);
            if (salesPageOrder == null)
                return NotFound();

            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);
            if (salesPageRecord == null)
                return NotFound();

            return new RedirectResult(_webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord)
                    + (!string.IsNullOrEmpty(salesPageOrder.AgentPId) ? "/" + salesPageOrder.AgentPId : string.Empty)
                    + "?msg=payment.failed");
        }
    }
}