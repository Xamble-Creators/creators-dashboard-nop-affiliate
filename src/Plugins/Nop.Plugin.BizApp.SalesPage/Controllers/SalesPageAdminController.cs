using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Tax;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Factories;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class SalesPageAdminController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly SalesPageRecordModelFactory _salesPageRecordModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly SalesPageRoxyFilemanService _salesPageRoxyFilemanService;

        #endregion

        #region Ctor

        public SalesPageAdminController(
            IPermissionService permissionService,
            SalesPageRecordService salesPageRecordService,
            SalesPageRecordModelFactory salesPageRecordModelFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            BizAppHttpClient bizAppHttpClient,
            IWebHelper webHelper,
            ICustomerService customerService,
            SalesPageRoxyFilemanService salesPageRoxyFilemanService
            )
        {
            _permissionService = permissionService;
            _salesPageRecordService = salesPageRecordService;
            _salesPageRecordModelFactory = salesPageRecordModelFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _bizAppHttpClient = bizAppHttpClient;
            _webHelper = webHelper;
            _customerService = customerService;
            _salesPageRoxyFilemanService = salesPageRoxyFilemanService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        #region Sales Page

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List(int customerId = 0)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //prepare model
            var model = await _salesPageRecordModelFactory.PrepareSalesPageRecordSearchModelAsync(new SalesPageRecordSearchModel()
            {
                CustomerId = customerId
            });

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageAdmin/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SalesPageList(SalesPageRecordSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _salesPageRecordModelFactory.PrepareSalesPageRecordListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //prepare model
            var model = await _salesPageRecordModelFactory.PrepareSalesPageRecordModelAsync(new SalesPageRecordModel(), null);

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageAdmin/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(SalesPageRecordModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //do not allow to create for bizapp admin
            if (await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return AccessDeniedView();

            if (!string.IsNullOrWhiteSpace(model.UrlSlug) && await _urlRecordService.GetBySlugAsync(model.UrlSlug) != null)
            {
                ModelState.AddModelError(string.Empty, "Url is already in used");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var maximumSalesPageAllowed = await _genericAttributeService.GetAttributeAsync<int>(customer, SalesPageDefaults.BizAppSalesPageLimitAttribute);
            if (maximumSalesPageAllowed <= (await _salesPageRecordService.GetAllSalesRecordsAsync(customerId: customer.Id)).Count)
            {
                ModelState.AddModelError(string.Empty, $"You are only allowed to create more than {maximumSalesPageAllowed} sales page");
            }

            if (ModelState.IsValid)
            {
                var salesPageRecord = new SalesPageRecord()
                {
                    CustomerId = customer.Id,
                    PageName = model.PageName,
                    PageHtmlContent = model.PageHtmlContent,
                    ImageKey = model.ImageKey,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _salesPageRecordService.InsertRecordAsync(salesPageRecord);

                var urlSlug = await _urlRecordService.ValidateSeNameAsync(salesPageRecord, model.UrlSlug, model.UrlSlug, false);
                await _urlRecordService.SaveSlugAsync(salesPageRecord, urlSlug, 0);

                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.PaymentTypeAttribute, SalesPageDefaults.BizAppPaySystemName);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.EnableLinkAffiliateAttribute, model.EnableLinkAffiliate);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ActivateSplitPaymentToAffiliateAttribute, model.ActivateSplitPaymentToAffiliate);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaSmsAttribute, model.NotificationToCustomerViaSms);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaWhatsAppAttribute, model.NotificationToCustomerViaWhatsApp);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaEmailAttribute, model.NotificationToCustomerViaEmail);
                await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ProductSkusAttribute, JsonConvert.SerializeObject(model.SelectedProductSkus));

                //var maintainSalesPageResponse = await _bizAppHttpClient.RequestAsync<MaintainSalesPageRequest, MaintainSalesPageResponse>(new MaintainSalesPageRequest()
                //{
                //    Customer = await _workContext.GetCurrentCustomerAsync(),
                //    Action = MaintainSalesPageRequest.ActionNew,
                //    Products = model.SelectedProductSkus.Select(x => new MaintainSalesPageRequest.ProductData()
                //    {
                //        ProductSku = x
                //    }).ToList(),
                //    SalesPageId = salesPageRecord.Id,
                //    SalesPageName = salesPageRecord.PageName,
                //    Url = _webHelper.GetStoreLocation() + urlSlug,
                //    OptionLinkAffiliate = model.EnableLinkAffiliate ? "Y" : "N",
                //    OptionSplitPayment = model.ActivateSplitPaymentToAffiliate ? "Y" : "N",
                //    OptionNotifyEmail = model.NotificationToCustomerViaEmail ? "Y" : "N",
                //    OptionNotifySms = model.NotificationToCustomerViaSms ? "Y" : "N",
                //    OptionNotifyWhatsApp = model.NotificationToCustomerViaWhatsApp ? "Y" : "N"
                //});

                //if (!maintainSalesPageResponse.Success)
                //    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.FailedToCreateSalesPageInBizAp"));
                //else
                //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.SalesPageAdded"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = salesPageRecord.Id });
            }

            //prepare model
            model = await _salesPageRecordModelFactory.PrepareSalesPageRecordModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageAdmin/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(id);
            if (salesPageRecord == null)
                return RedirectToAction("List");

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return RedirectToAction("List");
            }

            //prepare model
            var model = await _salesPageRecordModelFactory.PrepareSalesPageRecordModelAsync(null, salesPageRecord);

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageAdmin/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Edit(SalesPageRecordModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(model.Id);
            if (salesPageRecord == null)
                return RedirectToAction("List");

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return RedirectToAction("List");
            }

            if (!string.IsNullOrWhiteSpace(model.UrlSlug))
            {
                var urlRecord = await _urlRecordService.GetBySlugAsync(model.UrlSlug);
                if (urlRecord != null
                    && !(urlRecord.EntityId == salesPageRecord.Id && urlRecord.EntityName == salesPageRecord.GetType().Name))
                    ModelState.AddModelError(string.Empty, "Url is already in used");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    salesPageRecord.PageName = model.PageName;
                    salesPageRecord.PageHtmlContent = model.PageHtmlContent;
                    await _salesPageRecordService.UpdateRecordAsync(salesPageRecord);

                    var urlSlug = await _urlRecordService.ValidateSeNameAsync(salesPageRecord, model.UrlSlug, model.UrlSlug, false);
                    await _urlRecordService.SaveSlugAsync(salesPageRecord, urlSlug, 0);

                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.PaymentTypeAttribute, SalesPageDefaults.BizAppPaySystemName);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.EnableLinkAffiliateAttribute, model.EnableLinkAffiliate);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ActivateSplitPaymentToAffiliateAttribute, model.ActivateSplitPaymentToAffiliate);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaSmsAttribute, model.NotificationToCustomerViaSms);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaWhatsAppAttribute, model.NotificationToCustomerViaWhatsApp);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaEmailAttribute, model.NotificationToCustomerViaEmail);
                    await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ProductSkusAttribute, JsonConvert.SerializeObject(model.SelectedProductSkus));

                    //var maintainSalesPageResponse = await _bizAppHttpClient.RequestAsync<MaintainSalesPageRequest, MaintainSalesPageResponse>(new MaintainSalesPageRequest()
                    //{
                    //    Customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId),
                    //    Action = MaintainSalesPageRequest.ActionUpdate,
                    //    Products = model.SelectedProductSkus.Select(x => new MaintainSalesPageRequest.ProductData()
                    //    {
                    //        ProductSku = x
                    //    }).ToList(),
                    //    SalesPageId = salesPageRecord.Id,
                    //    SalesPageName = salesPageRecord.PageName,
                    //    Url = _webHelper.GetStoreLocation() + urlSlug,
                    //    OptionLinkAffiliate = model.EnableLinkAffiliate ? "Y" : "N",
                    //    OptionSplitPayment = model.ActivateSplitPaymentToAffiliate ? "Y" : "N",
                    //    OptionNotifyEmail = model.NotificationToCustomerViaEmail ? "Y" : "N",
                    //    OptionNotifySms = model.NotificationToCustomerViaSms ? "Y" : "N",
                    //    OptionNotifyWhatsApp = model.NotificationToCustomerViaWhatsApp ? "Y" : "N"
                    //});

                    //if (!maintainSalesPageResponse.Success)
                    //    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.FailedToUpdateSalesPageInBizAp"));
                    //else
                    //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.SalesPageUpdated"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = salesPageRecord.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }

            //prepare model
            model = await _salesPageRecordModelFactory.PrepareSalesPageRecordModelAsync(model, salesPageRecord, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageAdmin/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(id);
            if (salesPageRecord == null)
                return RedirectToAction("List");

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return RedirectToAction("List");
            }

            var seName = await _urlRecordService.GetSeNameAsync(salesPageRecord);
            var urlRecord = await _urlRecordService.GetBySlugAsync(seName);

            var productSkus = JsonConvert.DeserializeObject<IList<string>>(await _genericAttributeService.GetAttributeAsync<string>(salesPageRecord, SalesPageDefaults.ProductSkusAttribute));

            await _urlRecordService.DeleteUrlRecordsAsync(new UrlRecord[] { urlRecord });

            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.PaymentTypeAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.EnableLinkAffiliateAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ActivateSplitPaymentToAffiliateAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaSmsAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaWhatsAppAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaEmailAttribute, string.Empty);
            await _genericAttributeService.SaveAttributeAsync(salesPageRecord, SalesPageDefaults.ProductSkusAttribute, string.Empty);

            await _salesPageRecordService.DeleteRecordAsync(salesPageRecord);

            //var maintainSalesPageResponse = await _bizAppHttpClient.RequestAsync<MaintainSalesPageRequest, MaintainSalesPageResponse>(new MaintainSalesPageRequest()
            //{
            //    Customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId),
            //    Action = MaintainSalesPageRequest.ActionDelete,
            //    Products = productSkus.Select(x => new MaintainSalesPageRequest.ProductData()
            //    {
            //        ProductSku = x
            //    }).ToList(),
            //    SalesPageId = salesPageRecord.Id,
            //    SalesPageName = salesPageRecord.PageName,
            //    Url = _webHelper.GetStoreLocation() + seName,
            //    OptionLinkAffiliate = "N",
            //    OptionSplitPayment = "N",
            //    OptionNotifyEmail = "N",
            //    OptionNotifySms = "N",
            //    OptionNotifyWhatsApp = "N"
            //});

            foreach (var directory in _salesPageRoxyFilemanService.GetDirectoryList(salesPageRecord.ImageKey, "image"))
            {
                foreach (var file in _salesPageRoxyFilemanService.GetFiles(directory.GetType().GetProperty("p").GetValue(directory, null).ToString(), "image"))
                    _salesPageRoxyFilemanService.DeleteFile(file.GetType().GetProperty("p").GetValue(file, null).ToString());
            }

            foreach (var directoryPath in _salesPageRoxyFilemanService.GetDirectoryList(salesPageRecord.ImageKey, "image")
                .Select(d => d.GetType().GetProperty("p").GetValue(d, null).ToString())
                .OrderByDescending(x => x.Split('/').Count()))
                _salesPageRoxyFilemanService.DeleteDirectory(directoryPath);

            if (_webHelper.IsAjaxRequest(Request))
                return new NullJsonResult();

            //if (!maintainSalesPageResponse.Success)
            //    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.FailedToDeleteSalesPageInBizAp"));
            //else
            //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.SalesPageDeleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}