using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
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
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrderAdmin;
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
    public class SalesPageOrderAdminController : BasePluginController
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
        private readonly SalesPageOrderAdminModelFactory _orderModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly SalesPageOrderService _salesPageOrderService;

        #endregion

        #region Ctor

        public SalesPageOrderAdminController(
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
            SalesPageRoxyFilemanService salesPageRoxyFilemanService,
            SalesPageOrderAdminModelFactory orderModelFactory,
            IDateTimeHelper dateTimeHelper,
            SalesPageOrderService salesPageOrderService
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
            _orderModelFactory = orderModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _salesPageOrderService = salesPageOrderService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        #region Order list

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List(List<int> paymentStatuses = null)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return AccessDeniedView();

            //prepare model
            var model = await _orderModelFactory.PrepareOrderSearchModelAsync(new OrderSearchModel
            { 
                PaymentStatusIds = paymentStatuses
            });

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageOrderAdmin/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> OrderList(OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _orderModelFactory.PrepareOrderListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("ExportExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportExcelAll(OrderSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;

            //load orders
            var orders = await _salesPageOrderService.SearchOrdersAsync(
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                psIds: paymentStatusIds,
                phone: model.Phone,
                email: model.Email,
                customerName: model.CustomerName);

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.NoOrders"));
                return RedirectToAction("List");
            }

            try
            {
                var bytes = await _salesPageOrderService.ExportOrdersToXlsxAsync(orders);
                return File(bytes, MimeTypes.TextXlsx, "orders.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #endregion
    }
}