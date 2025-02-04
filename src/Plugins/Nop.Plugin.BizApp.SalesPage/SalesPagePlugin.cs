using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Plugin.BizApp.SalesPage.Components;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage
{
    public class SalesPagePlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public SalesPagePlugin(IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IPermissionService permissionService,
            ICustomerService customerService,
            IWorkContext workContext,
            WidgetSettings widgetSettings)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _permissionService = permissionService;
            _customerService = customerService;
            _workContext = workContext;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            if (await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
            {
                return new List<string> { AdminWidgetZones.DashboardBottom };
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets a type of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));
            
            if (widgetZone == AdminWidgetZones.DashboardBottom)
            {
                if (_permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage).Result)
                {
                    return typeof(WidgetsSalesPageDashboardViewComponent);
                }
            }
            return null;
        }

        /// <summary>
        /// Manage sitemap. You can use "SystemName" of menu items to manage existing sitemap or add a new menu item.
        /// </summary>
        /// <param name="rootNode">Root node of the sitemap.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var bizAppUsersRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);
            var bizAppAdminsRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppAdminsRoleName);
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());

            if (customerRoleIds.Contains(bizAppUsersRole.Id)
                || customerRoleIds.Contains(bizAppAdminsRole.Id))
            {
                rootNode.ChildNodes.Insert(rootNode.ChildNodes.Count - 1, new SiteMapNode
                {
                    Visible = true,
                    SystemName = "BizApp SalesPage",
                    Title = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.SalesPage"),
                    IconClass = "far fa-dot-circle",
                    ControllerName = "SalesPageAdmin",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary { { "area", AreaNames.Admin } }
                });
            }

            if (customerRoleIds.Contains(bizAppAdminsRole.Id))
            {
                rootNode.ChildNodes.Insert(rootNode.ChildNodes.Count - 1, new SiteMapNode
                {
                    Visible = true,
                    SystemName = "BizApp Users",
                    Title = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.Users"),
                    IconClass = "far fa-dot-circle",
                    ControllerName = "SalesPageUser",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary { { "area", AreaNames.Admin } }
                });

                rootNode.ChildNodes.Insert(rootNode.ChildNodes.Count - 1, new SiteMapNode
                {
                    Visible = true,
                    SystemName = "BizApp Orders",
                    Title = await _localizationService.GetResourceAsync("Admin.Orders"),
                    IconClass = "far fa-dot-circle",
                    ControllerName = "SalesPageOrderAdmin",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary { { "area", AreaNames.Admin } }
                });
            }
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            var salesPagePermissionProvider = new SalesPagePermissionProvider();
            await _permissionService.InstallPermissionsAsync(salesPagePermissionProvider);

            var bizAppUsers = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);
            var bizAppAdmins = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppAdminsRoleName);
            var accessAdminPanelPermission = (await _permissionService.GetAllPermissionRecordsAsync())
                .First(x => x.SystemName == StandardPermissionProvider.AccessAdminPanel.SystemName);
            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping()
            {
                CustomerRoleId = bizAppUsers.Id,
                PermissionRecordId = accessAdminPanelPermission.Id
            });
            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping()
            {
                CustomerRoleId = bizAppAdmins.Id,
                PermissionRecordId = accessAdminPanelPermission.Id
            });

            var htmlEditorManagePicturesPermission = (await _permissionService.GetAllPermissionRecordsAsync())
                .First(x => x.SystemName == StandardPermissionProvider.HtmlEditorManagePictures.SystemName);
            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping()
            {
                CustomerRoleId = bizAppUsers.Id,
                PermissionRecordId = htmlEditorManagePicturesPermission.Id
            });
            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping()
            {
                CustomerRoleId = bizAppAdmins.Id,
                PermissionRecordId = htmlEditorManagePicturesPermission.Id
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(SalesPageDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(SalesPageDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.BizApp.SalesPage.SalesPage"] = "Sales Page",
                ["Plugins.BizApp.SalesPage.BizAppApiUrl"] = "BizApp Api Url",
                ["Plugins.BizApp.SalesPage.BizAppApiUrl.Required"] = "BizApp Api Url is required",
                ["Plugins.BizApp.SalesPage.PageName"] = "Page Name",
                ["Plugins.BizApp.SalesPage.Url"] = "Url",
                ["Plugins.BizApp.SalesPage.UrlSlug"] = "Url",
                ["Plugins.BizApp.SalesPage.UrlSlug.Required"] = "Url is required",
                ["Plugins.BizApp.SalesPage.PageName.Required"] = "Page Name is required",
                ["Plugins.BizApp.SalesPage.PageHtmlContent"] = "Content/Copy Writing",
                ["Plugins.BizApp.SalesPage.PageHtmlContent.Required"] = "Content/Copy Writing is required",
                ["Plugins.BizApp.SalesPage.ProductIds"] = "Products",
                ["Plugins.BizApp.SalesPage.ProductIds.Required"] = "Products is required",
                ["Plugins.BizApp.SalesPage.PaymentType"] = "Payment Type",
                ["Plugins.BizApp.SalesPage.OtherSettings"] = "Other Settings",
                ["Plugins.BizApp.SalesPage.EnableLinkAffiliate"] = "Enable link affiliate",
                ["Plugins.BizApp.SalesPage.ActivateSplitPaymentToAffiliate"] = "Activate split payment to affiliate",
                ["Plugins.BizApp.SalesPage.NotificationToCustomer"] = "Notification To Buyer/Customer",
                ["Plugins.BizApp.SalesPage.NotificationToCustomerViaSms"] = "Via Sms",
                ["Plugins.BizApp.SalesPage.NotificationToCustomerViaWhatsApp"] = "Via WhatsApp",
                ["Plugins.BizApp.SalesPage.NotificationToCustomerViaEmail"] = "Via Email",
                ["Plugins.BizApp.SalesPage.AddNew"] = "Add New Sales Page",
                ["Plugins.BizApp.SalesPage.BackToList"] = "Back to list",
                ["Plugins.BizApp.SalesPage.EditSalesPageDetails"] = "Edit Sales Page Details",
                ["Plugins.BizApp.SalesPage.Info"] = "Info",
                ["Plugins.BizApp.SalesPage.Product"] = "Product",
                ["Plugins.BizApp.SalesPage.Setting"] = "Setting",
                ["Plugins.BizApp.SalesPage.NoProductsAvailable"] = "No Products Available",
                ["Plugins.BizApp.SalesPage.Fpx"] = "Fpx / CC (BIZAPPAY)",
                ["Plugins.BizApp.SalesPage.SalesPageAdded"] = "The new sales page has been added successfully.",
                ["Plugins.BizApp.SalesPage.SalesPageUpdated"] = "The sales page has been updated successfully.",
                ["Plugins.BizApp.SalesPage.SalesPageDeleted"] = "The sales page has been deleted successfully.",
                ["Plugins.BizApp.SalesPage.Postage"] = "Postage",
                ["Plugins.BizApp.SalesPage.Postage.Required"] = "Postage is required",
                ["Plugins.BizApp.SalesPage.FullName"] = "Full Name",
                ["Plugins.BizApp.SalesPage.FullName.Required"] = "Full Name is required",
                ["Plugins.BizApp.SalesPage.Checkout.Products.Required"] = "Full Name is required",
                ["Plugins.BizApp.SalesPage.FailedToCreateSalesPageInBizApp"] = "Failed to create sales page in BizApp",
                ["Plugins.BizApp.SalesPage.FailedToUpdateSalesPageInBizApp"] = "Failed to update sales page in BizApp",
                ["Plugins.BizApp.SalesPage.FailedToDeleteSalesPageInBizApp"] = "Failed to delete sales page in BizApp",
                ["Admin.SalesReport.SalesStatistics.Alert.FailedLoad"] = "Failed to load statistics.",
                ["Admin.SalesReport.SalesStatistics"] = "Sales",
                ["Admin.SalesReport.SalesStatistics.Year"] = "Year",
                ["Admin.SalesReport.SalesStatistics.Month"] = "Month",
                ["Admin.SalesReport.SalesStatistics.Week"] = "Week",
                ["Admin.SalesReport.SalesStatistics.Profits"] = "Profits",
                ["Plugins.BizApp.SalesPage.Users"] = "Users",
                ["Plugins.BizApp.SalesPage.User"] = "User",
                ["Plugins.BizApp.SalesPage.Visits"] = "Visits",
                ["Plugins.BizApp.SalesPage.VisitsStatistics.Alert.FailedLoad"] = "Failed to load statistics.",
                ["Plugins.BizApp.SalesPage.NoSalesPage"] = "No Sales Page",
                ["Plugins.BizApp.SalesPage.DashboardFor"] = "Dashboard for ",
                ["Plugins.BizApp.SalesPage.PleaseSelect"] = "- Please Select -",
                ["Plugins.BizApp.SalesPage.PleaseSelectUser"] = "- Please Select User -",
                ["Plugins.BizApp.SalesPage.SalesPageFailedToLoad"] = "Sales page failed to load",
                ["Plugins.BizApp.SalesPage.Address"] = "Address",
                ["Plugins.BizApp.SalesPage.BizAppOrderId"] = "BIZAPP ORDERID",
                ["Plugins.BizApp.SalesPage.FullName.MinLength"] = "FullName should have at least 5 characters.",
                ["Plugins.BizApp.SalesPage.ZipPostalCode.MinLength"] = "Zip / postal code should be 5 characters."
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(SalesPageDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);

            var bizAppUsers = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);
            var bizAppAdmins = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppAdminsRoleName);
            var accessAdminPanelPermission = (await _permissionService.GetAllPermissionRecordsAsync())
                .First(x => x.SystemName == StandardPermissionProvider.AccessAdminPanel.SystemName);
            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(accessAdminPanelPermission.Id,
                bizAppUsers.Id);
            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(accessAdminPanelPermission.Id,
                bizAppAdmins.Id);

            var htmlEditorManagePicturesPermission = (await _permissionService.GetAllPermissionRecordsAsync())
                .First(x => x.SystemName == StandardPermissionProvider.HtmlEditorManagePictures.SystemName);
            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(htmlEditorManagePicturesPermission.Id,
                bizAppUsers.Id);
            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(htmlEditorManagePicturesPermission.Id,
                bizAppAdmins.Id);

            var salesPagePermissionProvider = new SalesPagePermissionProvider();
            foreach (var permission in salesPagePermissionProvider.GetPermissions())
                await _permissionService.DeletePermissionRecordAsync(permission);

            await _settingService.DeleteSettingAsync<SalesPageSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.BizApp.SalesPage");

            await base.UninstallAsync();
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;
    }
}