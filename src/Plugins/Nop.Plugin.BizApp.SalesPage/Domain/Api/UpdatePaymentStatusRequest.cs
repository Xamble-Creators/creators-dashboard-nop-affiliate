using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class UpdatePaymentStatusRequest : ApiRequest, IAuthorizedRequest
    {
        [JsonProperty(propertyName: "salespageid")]
        public int SalesPageId { get; set; }

        [JsonProperty(propertyName: "bizapporderid")]
        public string BizAppOrderId { get; set; }

        [JsonProperty(propertyName: "paymentstatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty(propertyName: "paymentamount")]
        public string PaymentAmount { get; set; }

        [JsonProperty(propertyName: "paymenturl")]
        public string PaymentUrl { get; set; }

        [JsonProperty(propertyName: "collectionid")]
        public string CollectionId { get; set; }

        [JsonProperty(propertyName: "billid")]
        public string BillId { get; set; }

        [JsonProperty(propertyName: "fpxkey")]
        public string FpxKey { get; set; }


        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "paymentstatus";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}