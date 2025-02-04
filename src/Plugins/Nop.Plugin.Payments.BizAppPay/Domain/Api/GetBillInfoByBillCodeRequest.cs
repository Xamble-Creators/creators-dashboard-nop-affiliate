using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public class  GetBillInfoByBillCodeRequest : ApiRequest
    {
        [JsonProperty("search_str")]
        public string SearchString { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "api/v3/bill/info";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}