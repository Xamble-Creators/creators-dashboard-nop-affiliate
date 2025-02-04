using System;
using Nop.Core;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class SalesPageOrder : BaseEntity
    {
        public int OrderId { get; set; }

        public string AgentPId { get; set; }

        public int SalesPageRecordId { get; set; }

        public string BizAppOrderId { get; set; }

        public string PaymentUrl { get; set; }

        public string BillId { get; set; }

        public string CollectionId { get; set; }

        public string FpxKey { get; set; }

        public decimal TotalAgentPrice { get; set; }

        public decimal AgentCommission { get; set; }
    }
}