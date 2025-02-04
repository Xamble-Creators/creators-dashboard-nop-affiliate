using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
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
    public class SalesPageReportService
    {
        #region Fields

        private readonly IRepository<SalesPageOrderItem> _salesPageOrderItemRepository;
        private readonly IRepository<SalesPageOrder> _salesPageOrderRepository;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        public SalesPageReportService(
            IRepository<SalesPageOrderItem> salesPageOrderItemRepository,
            IRepository<SalesPageOrder> salesPageOrderRepository,
            IRepository<Order> orderRepository)
        {
            _salesPageOrderItemRepository = salesPageOrderItemRepository;
            _salesPageOrderRepository = salesPageOrderRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public virtual async Task<IPagedList<SalesStatisticsItem>> GetSalesStatisticsItemsAsync(int salesPageRecordId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _orderRepository.Table.Where(x => x.PaymentStatusId == (int)PaymentStatus.Paid);

            if (salesPageRecordId > 0)
                query = from o in _orderRepository.Table
                        join spo in _salesPageOrderRepository.Table on o.Id equals spo.OrderId
                        where spo.SalesPageRecordId == salesPageRecordId
                        select o;

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            var ssQuery = from o in query
                          join spo in _salesPageOrderRepository.Table on o.Id equals spo.OrderId
                          select new SalesStatisticsItem()
                          {
                              OrderTotal = o.OrderTotal,
                              Profit = o.OrderTotal
                                - spo.TotalAgentPrice
                                - o.OrderShippingInclTax
                          };

            //database layer paging
            return await ssQuery.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        #endregion
    }
}