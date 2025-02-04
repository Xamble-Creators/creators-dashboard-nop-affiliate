using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public class BillInfoResponse : ApiResponse
    {
        public BillData Bill { get; set; }

        public class BillData
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string CategoryCode { get; set; }

            public string BillCode { get; set; }

            [JsonProperty("webreturn")]
            public string WebReturnUrl { get; set; }

            [JsonProperty("servicereturn")]
            public string ServiceReturnUrl { get; set; }

            public string Ref { get; set; }

            public string MultiPayment { get; set; }

            public DateTime Datetime { get; set; }

            public IList<PaymentData> Payments { get; set; }

            public class PaymentData
            {
                public string Status { get; set; }

                public decimal PaidAmount { get; set; }

                public decimal NettAmount { get; set; }

                public string Invoice { get; set; }

                public string PayerName { get; set; }

                public string PayerEmail { get; set; }

                public string PayerPhone { get; set; }

                public DateTime Datetime { get; set; }

                public string PaidVia { get; set; }
            }
        }
    }
}