using System;
using Nop.Core;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class SalesPageOrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        public string ProductSku { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal AgentPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal TotalAgentPrice { get; set; }
    }
}