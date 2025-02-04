using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public record PaymentCallback
    {
        public string BillCode { get; set; }

        public decimal BillAmount { get; set; }

        public string BillStatus { get; set; }

        public string BillInvoice { get; set; }

        public string BillTrans { get; set; }
    }
}