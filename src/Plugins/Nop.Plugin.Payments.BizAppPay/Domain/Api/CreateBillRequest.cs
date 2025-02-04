using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public class CreateBillRequest : ApiRequest
    {
        [JsonProperty(propertyName: "category")]
        public string Category { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "amount")]
        public string Amount { get; set; }

        [JsonProperty(propertyName: "payer_name")]
        public string PayerName { get; set; }

        [JsonProperty(propertyName: "payer_email")]
        public string PayerEmail { get; set; }

        [JsonProperty(propertyName: "payer_phone")]
        public string PayerPhone { get; set; }

        [JsonProperty(propertyName: "webreturn_url")]
        public string WebReturnUrl { get; set; }

        [JsonProperty(propertyName: "callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty(propertyName: "ext_reference")]
        public string ReferenceNumber { get; set; }

        [JsonProperty(propertyName: "splitArgs")]
        public string SplitArgs { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "api/v3/bill/create";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}