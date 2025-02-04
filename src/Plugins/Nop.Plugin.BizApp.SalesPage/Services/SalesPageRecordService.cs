using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Services.Catalog;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public class SalesPageRecordService
    {
        #region Fields

        private readonly IRepository<SalesPageRecord> _salesPageRecordRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public SalesPageRecordService(
            IRepository<SalesPageRecord> salesPageRecordRepository,
            IStaticCacheManager staticCacheManager)
        {
            _salesPageRecordRepository = salesPageRecordRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<SalesPageRecord> GetRecordByIdAsync(int id)
        {
            return await _salesPageRecordRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<SalesPageRecord> GetRecordByImageKeyAsync(string imageKey)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(SalesPageDefaults.SalesPageByImageKeyKeyCache,
                imageKey);

            return await _staticCacheManager.GetAsync(key, async () =>
            {
                return await _salesPageRecordRepository.Table.FirstOrDefaultAsync(x => x.ImageKey == imageKey);
            });
        }

        public async Task InsertRecordAsync(SalesPageRecord record)
        {
            await _salesPageRecordRepository.InsertAsync(record);
        }

        public async Task UpdateRecordAsync(SalesPageRecord record)
        {
            await _salesPageRecordRepository.UpdateAsync(record);
        }

        public async Task DeleteRecordAsync(SalesPageRecord record)
        {
            await _salesPageRecordRepository.DeleteAsync(record);
        }

        public async Task<IPagedList<SalesPageRecord>> GetAllSalesRecordsAsync(int? customerId = null, string pageName = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await _salesPageRecordRepository.GetAllPagedAsync(query =>
            {
                if (customerId.HasValue)
                    query = query.Where(record => record.CustomerId == customerId);

                if (!string.IsNullOrEmpty(pageName))
                    query = query.Where(record => record.PageName.Contains(pageName));

                query = query.OrderBy(record => record.PageName);

                return query;
            }, pageIndex, pageSize);
        }

        #endregion
    }
}