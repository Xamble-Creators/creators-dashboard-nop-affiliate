using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Plugin.BizApp.SalesPage.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public class SalesPageProductService
    {
        #region Fields

        private readonly IRepository<SalesPageOrderItem> _salesPageOrderItemRepository;
        private readonly IRepository<SalesPageOrder> _salesPageOrderRepository;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ILanguageService _languageService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public SalesPageProductService(
            IRepository<SalesPageOrderItem> salesPageOrderItemRepository,
            IRepository<SalesPageOrder> salesPageOrderRepository,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            BizAppHttpClient bizAppHttpClient,
            SalesPageRecordService salesPageRecordService,
            IAddressService addressService,
            IOrderService orderService,
            ICustomNumberFormatter customNumberFormatter,
            IWorkContext workContext,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            ILanguageService languageService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IEventPublisher eventPublisher,
            IStaticCacheManager staticCacheManager)
        {
            _salesPageOrderItemRepository = salesPageOrderItemRepository;
            _salesPageOrderRepository = salesPageOrderRepository;
            _orderProcessingService = orderProcessingService;
            _customerService = customerService;
            _bizAppHttpClient = bizAppHttpClient;
            _salesPageRecordService = salesPageRecordService;
            _addressService = addressService;
            _orderService = orderService;
            _customNumberFormatter = customNumberFormatter;
            _workContext = workContext;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _languageService = languageService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _urlRecordService = urlRecordService;
            _genericAttributeService = genericAttributeService;
            _eventPublisher = eventPublisher;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<ProductListResponse> GetAllProductsAsync(Customer customer, string agentPId = null)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(SalesPageDefaults.ProductsKeyCache,
                customer.Id, agentPId);
            key.CacheTime = SalesPageDefaults.ProductsKeyCache.CacheTime;

            return await _staticCacheManager.GetAsync(key, async () =>
            {
                return await _bizAppHttpClient.RequestAsync<GetProductListRequest, ProductListResponse>(new GetProductListRequest()
                {
                    Customer = customer,
                    AgentPId = agentPId
                });
            });
        }

        #endregion
    }
}