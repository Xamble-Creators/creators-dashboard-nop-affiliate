using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageDashboard;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.BizApp.SalesPage.Components
{
    public class WidgetsSalesPageDashboardViewComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        public WidgetsSalesPageDashboardViewComponent(IPermissionService permissionService,
            SalesPageRecordService salesPageRecordService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _salesPageRecordService = salesPageRecordService;
            _workContext = workContext;
            _localizationService = localizationService;
            _customerService = customerService;
        }

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>View component result</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new DashboardModel();

            var bizAppUsersRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);
            model.AvailableCustomers = (await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { bizAppUsersRole.Id }))
                .Select(x => new SelectListItem() { Text = x.FirstName, Value = x.Id.ToString() })
                .ToList();

            model.AvailableCustomers.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PleaseSelect"), Value = string.Empty });

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                model.SelectedCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            }

            if (model.SelectedCustomerId.HasValue)
            {
                var salesPageRecords = await _salesPageRecordService.GetAllSalesRecordsAsync(customerId: model.SelectedCustomerId);
                model.AvailableSalesPages = salesPageRecords.Select(x => new SelectListItem()
                {
                    Text = x.PageName,
                    Value = x.Id.ToString()
                }).ToList();

                if (salesPageRecords.TotalCount == 0)
                    model.AvailableSalesPages.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.NoSalesPage"), Value = string.Empty });
            }
            else
            {
                model.AvailableSalesPages.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PleaseSelectUser"), Value = string.Empty });
            }


            return View("~/Plugins/BizApp.SalesPage/Views/Components/WidgetsSalesPageDashboard/Dashboard.cshtml", model);
        }
    }
}