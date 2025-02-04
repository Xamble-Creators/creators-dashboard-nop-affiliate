using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
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
    public class SalesPageVisitService
    {
        #region Fields

        private readonly IRepository<SalesPageVisit> _salesPageVisitRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public SalesPageVisitService(
            IRepository<SalesPageVisit> salesPageVisitRepository,
            IStaticCacheManager staticCacheManager)
        {
            _salesPageVisitRepository = salesPageVisitRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<SalesPageVisit> AddUniqueVisitAsync(int salesPageRecordId, string ipAddress)
        {
            var salesPageVisit = await GetSalesPageVisitAsync(salesPageRecordId, ipAddress);
            if (salesPageVisit != null)
                return salesPageVisit;

            salesPageVisit = new SalesPageVisit()
            {
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = ipAddress,
                SalesPageRecordId = salesPageRecordId
            };

            await _salesPageVisitRepository.InsertAsync(salesPageVisit);

            return salesPageVisit;
        }

        public async Task<SalesPageVisit> GetSalesPageVisitAsync(int salesPageRecordId, string ipAddress)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(SalesPageDefaults.SalesPageVisitKeyCache,
                salesPageRecordId, ipAddress);

            return await _staticCacheManager.GetAsync(key, async () =>
            {
                return await _salesPageVisitRepository.Table.FirstOrDefaultAsync(x => x.SalesPageRecordId == salesPageRecordId
                    && x.IpAddress == ipAddress);
            });
        }

        public virtual async Task<IPagedList<SalesPageVisit>> SearchSalesPageVisitsAsync(int salesPageRecordId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _salesPageVisitRepository.Table;

            if (salesPageRecordId > 0)
                query = query.Where(o => o.SalesPageRecordId == salesPageRecordId);

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        #endregion
    }
}