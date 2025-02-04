using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin;
using Nop.Plugin.BizApp.SalesPage.Security;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.BizApp.SalesPage.Factories
{
    public partial class SalesPageRecordModelFactory
    {
        #region Fields

        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IUrlRecordService _urlRecordService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly SalesPageProductService _salesPageProductService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public SalesPageRecordModelFactory(
            SalesPageRecordService salesPageRecordService,
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IWebHelper webHelper,
            IUrlRecordService urlRecordService,
            BizAppHttpClient bizAppHttpClient,
            SalesPageProductService salesPageProductService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IProductService productService)
        {
            _salesPageRecordService = salesPageRecordService;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _workContext = workContext;
            _webHelper = webHelper;
            _urlRecordService = urlRecordService;
            _bizAppHttpClient = bizAppHttpClient;
            _salesPageProductService = salesPageProductService;
            _customerService = customerService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public virtual async Task<SalesPageRecordSearchModel> PrepareSalesPageRecordSearchModelAsync(SalesPageRecordSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var bizAppUsersRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);

            searchModel.AvailableCustomers = (await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { bizAppUsersRole.Id }))
                .Select(x => new SelectListItem() { Text = x.FirstName, Value = x.Id.ToString() })
                .ToList();

            searchModel.AvailableCustomers.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = string.Empty });


            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<SalesPageRecordListModel> PrepareSalesPageRecordListModelAsync(SalesPageRecordSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                searchModel.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;

            var salesPageRecords = await _salesPageRecordService.GetAllSalesRecordsAsync(customerId: searchModel.CustomerId,
                pageName: searchModel.SearchPageName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new SalesPageRecordListModel().PrepareToGridAsync(searchModel, salesPageRecords, () =>
            {
                return salesPageRecords.SelectAwait(async salesPageRecord =>
                {
                    //fill in model values from the entity
                    var salesPageRecordModel = new SalesPageRecordModel();

                    salesPageRecordModel.Id = salesPageRecord.Id;
                    salesPageRecordModel.PageName = salesPageRecord.PageName;

                    salesPageRecordModel.UrlSlug = await _urlRecordService.GetSeNameAsync(salesPageRecord);
                    salesPageRecordModel.StoreUrl = _webHelper.GetStoreLocation();

                    var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);
                    salesPageRecordModel.CustomerName = customer.FirstName;

                    salesPageRecordModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(salesPageRecord.CreatedOnUtc, DateTimeKind.Utc);

                    return salesPageRecordModel;
                });
            });

            return model;
        }

        public virtual async Task<SalesPageRecordModel> PrepareSalesPageRecordModelAsync(SalesPageRecordModel model, SalesPageRecord salesPageRecord, bool excludeProperties = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (salesPageRecord != null)
            {
                customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);

                //fill in model values from the entity
                model ??= new SalesPageRecordModel();

                model.Id = salesPageRecord.Id;

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.ImageKey = salesPageRecord.ImageKey;
                    model.PageName = salesPageRecord.PageName;
                    model.PageHtmlContent = salesPageRecord.PageHtmlContent;

                    model.UrlSlug = await _urlRecordService.GetSeNameAsync(salesPageRecord);
                    model.StoreUrl = _webHelper.GetStoreLocation();

                    model.PaymentType = await _genericAttributeService.GetAttributeAsync<string>(salesPageRecord, SalesPageDefaults.PaymentTypeAttribute);
                    model.EnableLinkAffiliate = await _genericAttributeService.GetAttributeAsync<bool>(salesPageRecord, SalesPageDefaults.EnableLinkAffiliateAttribute);
                    model.ActivateSplitPaymentToAffiliate = await _genericAttributeService.GetAttributeAsync<bool>(salesPageRecord, SalesPageDefaults.ActivateSplitPaymentToAffiliateAttribute);
                    model.NotificationToCustomerViaSms = await _genericAttributeService.GetAttributeAsync<bool>(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaSmsAttribute);
                    model.NotificationToCustomerViaWhatsApp = await _genericAttributeService.GetAttributeAsync<bool>(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaWhatsAppAttribute);
                    model.NotificationToCustomerViaEmail = await _genericAttributeService.GetAttributeAsync<bool>(salesPageRecord, SalesPageDefaults.NotificationToCustomerViaEmailAttribute);
                    model.SelectedProductSkus = JsonConvert.DeserializeObject<IList<string>>(await _genericAttributeService.GetAttributeAsync<string>(salesPageRecord, SalesPageDefaults.ProductSkusAttribute));
                }
            }
            else
            {
                model.ImageKey = Guid.NewGuid().ToString();
                model.PaymentType = SalesPageDefaults.BizAppPaySystemName;
            }

            //var products = await _salesPageProductService.GetAllProductsAsync(customer);
            var products = await _productService.SearchProductsAsync();
            model.AvailableProductSkus = products.Select(x => new SelectListItem()
            {
                Text = x.Name + " (" + x.Sku + ")",
                Value = x.Sku
            }).ToList();

            return model;
        }

        #endregion
    }
}
