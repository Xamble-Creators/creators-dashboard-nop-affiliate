using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Factories;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.BizApp.SalesPage.Validators;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class SalesPageOrderController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly SalesPageOrderModelFactory _salesPageOrderModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly SalesPageOrderService _salesPageOrderService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly SalesPageVisitService _salesPageVisitService;

        #endregion

        #region Ctor

        public SalesPageOrderController(
            IPermissionService permissionService,
            SalesPageRecordService salesPageRecordService,
            SalesPageOrderModelFactory salesPageOrderModelFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            BizAppHttpClient bizAppHttpClient,
            ICustomerService customerService,
            IWebHelper webHelper,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IOrderProcessingService orderProcessingService,
            SalesPageOrderService salesPageOrderService,
            IOrderService orderService,
            IPaymentService paymentService,
            SalesPageVisitService salesPageVisitService)
        {
            _permissionService = permissionService;
            _salesPageRecordService = salesPageRecordService;
            _salesPageOrderModelFactory = salesPageOrderModelFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _bizAppHttpClient = bizAppHttpClient;
            _customerService = customerService;
            _webHelper = webHelper;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _orderProcessingService = orderProcessingService;
            _salesPageOrderService = salesPageOrderService;
            _orderService = orderService;
            _paymentService = paymentService;
            _salesPageVisitService = salesPageVisitService;
        }

        #endregion

        #region Utilities

        //replace <script>, </script> and /\?%*:|"<> to empty string as request by Hasnol on 17 Apr 2023
        private string ReplaceCustomCharacters(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            str = str.Replace("<script>", "")
                .Replace("</script>", "")
                .Replace("/[/\\\\?%*:|\"<>]/g", "")
                .Trim();

            return str;
        }

        #endregion

        #region Methods

        #region Sales Page

        public virtual async Task<IActionResult> Order(int id, string agentPId, string msg)
        {
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(id);

            if (salesPageRecord == null)
                return NotFound();

            //if (!string.IsNullOrEmpty(agentPId))
            //{
            //    var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);
            //    var validateSalesPageResponse = await _bizAppHttpClient.RequestAsync<ValidateSalesPageRequest, ValidateSalesPageResponse>(new ValidateSalesPageRequest()
            //    {
            //        AgentPId = agentPId,
            //        Customer = customer,
            //        SalesPageId = salesPageRecord.Id,
            //        Url = _webHelper.GetStoreLocation() + await _urlRecordService.GetSeNameAsync(salesPageRecord)
            //    });

            //    if (!validateSalesPageResponse.Success)
            //        return NotFound();
            //}

            var model = await _salesPageOrderModelFactory.PrepareSalesPageOrderModelAsync(new SalesPageOrderModel()
            {
                AgentPId = agentPId,
                FailMessageKey = msg
            }, salesPageRecord);

            await _salesPageVisitService.AddUniqueVisitAsync(id, _webHelper.GetCurrentIpAddress());

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageOrder/Order.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Order(SalesPageOrderModel model, IFormCollection form)
        {
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(model.SalesPageRecordId);

            if (salesPageRecord == null)
                return NotFound();

            var cartItems = form.Where(x => x.Key.Contains("ci_qty_"))
                .Select(x =>
                {
                    var cartItem = new PlaceOrderRequest.CartItem();
                    cartItem.ProductSku = x.Key.Substring(x.Key.IndexOf(x.Key.Split('_')[2]));

                    int.TryParse(x.Value, out int quantity);

                    cartItem.Quantity = quantity;

                    return cartItem;
                })
                .Where(x => x.Quantity > 0)
                .ToList();
            if (cartItems.Sum(x => x.Quantity) <= 0)
            {
                ModelState.AddModelError("", "You have no products to checkout");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.Address1 = ReplaceCustomCharacters(model.Address1);
                    model.Address2 = ReplaceCustomCharacters(model.Address2);
                    model.City = ReplaceCustomCharacters(model.City);
                    model.Email = ReplaceCustomCharacters(model.Email);
                    model.FullName = ReplaceCustomCharacters(model.FullName);
                    model.PhoneNumber = ReplaceCustomCharacters(model.PhoneNumber);
                    model.ZipPostalCode = ReplaceCustomCharacters(model.ZipPostalCode);

                    //validate again after replace characters
                    var salesPageOrderValidator = new SalesPageOrderValidator(_localizationService,
                        _countryService, _stateProvinceService);
                    var validationResult = salesPageOrderValidator.Validate(model);
                    if (validationResult.IsValid)
                    {
                        var order = await _salesPageOrderService.PlaceOrderAsync(new PlaceOrderRequest()
                        {
                            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                            SalesPageRecordId = salesPageRecord.Id,
                            Address1 = model.Address1,
                            Address2 = model.Address2,
                            AgentPId = model.AgentPId,
                            CartItems = cartItems,
                            City = model.City,
                            CountryId = model.CountryId,
                            Email = model.Email,
                            FullName = model.FullName,
                            PhoneNumber = model.PhoneNumber,
                            Postage = model.SelectedPostage,
                            StateProvinceId = model.StateProvinceId,
                            ZipPostalCode = model.ZipPostalCode
                        });

                        var postProcessPaymentRequest = new PostProcessPaymentRequest
                        {
                            Order = order
                        };
                        await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                        return RedirectToRoute("SalesPageThankYou", new { customOrderNumber = order.CustomOrderNumber });

                        //return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
                    }
                    else
                    {
                        validationResult.AddToModelState(ModelState);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            //prepare model
            model = await _salesPageOrderModelFactory.PrepareSalesPageOrderModelAsync(model,
                await _salesPageRecordService.GetRecordByIdAsync(model.SalesPageRecordId),
                true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageOrder/Order.cshtml", model);
        }

        public virtual async Task<IActionResult> ThankYou(string customOrderNumber)
        {
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(customOrderNumber);
            if (order == null)
                return NotFound();

            if (order.PaymentStatus != Nop.Core.Domain.Payments.PaymentStatus.Paid)
                return NotFound();

            var model = await _salesPageOrderModelFactory.PrepareOrderSummaryModelAsync(order);

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageOrder/ThankYou.cshtml", model);
        }

        public virtual async Task<IActionResult> SalesPageNotFound()
        {
            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageOrder/NotFound.cshtml");
        }

        #endregion

        #endregion
    }
}