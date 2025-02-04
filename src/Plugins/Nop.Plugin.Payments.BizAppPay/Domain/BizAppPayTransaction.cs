using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.BizAppPay.Domain
{
    public class BizAppPayTransaction : BaseEntity
    {
        public string CustomOrderNumber { get; set; }

        public string TransactionNumber { get; set; }
    }
}