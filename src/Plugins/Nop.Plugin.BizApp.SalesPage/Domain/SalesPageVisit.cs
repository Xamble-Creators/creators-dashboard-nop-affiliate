using System;
using Nop.Core;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class SalesPageVisit : BaseEntity
    {
        public int SalesPageRecordId { get; set; }

        public string IpAddress { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}