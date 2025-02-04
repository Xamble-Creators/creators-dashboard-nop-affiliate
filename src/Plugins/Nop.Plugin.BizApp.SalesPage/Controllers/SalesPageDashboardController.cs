using System;
using System.Collections.Generic;
using System.Globalization;
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
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class SalesPageDashboardController : BasePluginController
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
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly SalesPageReportService _salesPageReportService;
        private readonly SalesPageVisitService _salesPageVisitService;
        private readonly SalesPageRecordModelFactory _salesPageRecordModelFactory;

        #endregion

        #region Ctor

        public SalesPageDashboardController(
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
            IDateTimeHelper dateTimeHelper,
            SalesPageReportService salesPageReportService,
            SalesPageVisitService salesPageVisitService,
            SalesPageRecordModelFactory salesPageRecordModelFactory)
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
            _dateTimeHelper = dateTimeHelper;
            _salesPageReportService = salesPageReportService;
            _salesPageVisitService = salesPageVisitService;
            _salesPageRecordModelFactory = salesPageRecordModelFactory;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> GetSalesPageByCustomerId(int customerId)
        {
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return await AccessDeniedDataTablesJson();

            if (customerId == 0)
                return Json(new[] {
                    new { id = string.Empty, name = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PleaseSelectUser") }
                });

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                customerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            }

            //prepare model
            var searchModel = new SalesPageRecordSearchModel()
            {
                CustomerId = customerId,
                Length = int.MaxValue
            };

            var model = await _salesPageRecordModelFactory.PrepareSalesPageRecordListModelAsync(searchModel);

            if (model.RecordsTotal == 0)
                return Json(new[] {
                    new { id = string.Empty, name = await _localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.NoSalesPage") }
                });

            return Json(model.Data.Select(x => new { id = x.Id, name = x.PageName }));
        }

        public virtual async Task<IActionResult> LoadSalesStatistics(int salesPageRecordId, string period)
        {
            if (salesPageRecordId == 0)
                return Content(string.Empty);

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return Content(string.Empty);

            var salesPageCustomerId = 0;
            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageRecordId);
                if (salesPageRecord == null || salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return Content(string.Empty);

                salesPageCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            }

            var result = new List<object>();

            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();

            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        var statisticsItems = await _salesPageReportService.GetSalesStatisticsItemsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone));
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            totalProfit = statisticsItems.Sum(x => x.Profit),
                            totalSales = statisticsItems.Sum(x => x.OrderTotal)
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        var statisticsItems = await _salesPageReportService.GetSalesStatisticsItemsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone));
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            totalProfit = statisticsItems.Sum(x => x.Profit),
                            totalSales = statisticsItems.Sum(x => x.OrderTotal)
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        var statisticsItems = await _salesPageReportService.GetSalesStatisticsItemsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone));
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            totalProfit = statisticsItems.Sum(x => x.Profit),
                            totalSales = statisticsItems.Sum(x => x.OrderTotal)
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        public virtual async Task<IActionResult> LoadPageVisitStatistics(int salesPageRecordId, string period)
        {
            if (salesPageRecordId == 0)
                return Content(string.Empty);

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppSalesPage))
                return Content(string.Empty);

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageRecordId);
                if (salesPageRecord == null || salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return Content(string.Empty);
            }

            var result = new List<object>();

            var nowDt = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();

            var culture = new CultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = (await _salesPageVisitService.SearchSalesPageVisitsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                                pageIndex: 0, pageSize: 1, getOnlyTotalCount: true)).TotalCount
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = (await _salesPageVisitService.SearchSalesPageVisitsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                                pageIndex: 0, pageSize: 1, getOnlyTotalCount: true)).TotalCount
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = (await _salesPageVisitService.SearchSalesPageVisitsAsync(salesPageRecordId: salesPageRecordId,
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                                pageIndex: 0, pageSize: 1, getOnlyTotalCount: true)).TotalCount
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        #endregion
    }
}