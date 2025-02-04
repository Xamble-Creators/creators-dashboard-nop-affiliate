using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Factories;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class SalesPageUserController : BasePluginController
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
        private readonly ICustomerModelFactory _customerModelFactory;

        #endregion

        #region Ctor

        public SalesPageUserController(
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
            ICustomerModelFactory customerModelFactory)
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
            _customerModelFactory = customerModelFactory;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return AccessDeniedView();

            var bizAppUsersRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);

            //prepare model
            var model = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel() { 
                SelectedCustomerRoleIds = new List<int>() { bizAppUsersRole.Id }
            });

            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageUser/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UserList(CustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                return await AccessDeniedDataTablesJson();

            var bizAppUsersRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);

            searchModel.SelectedCustomerRoleIds = new List<int>() { bizAppUsersRole.Id };

            //prepare model
            var model = await _customerModelFactory.PrepareCustomerListModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}