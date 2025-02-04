using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public class CreateBillResponse : ApiResponse
    {
        public string BillCode { get; set; }


        public string Url { get; set; }
    }
}