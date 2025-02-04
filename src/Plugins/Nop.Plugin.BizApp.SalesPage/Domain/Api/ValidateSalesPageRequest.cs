using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class ValidateSalesPageRequest : ApiRequest, IAuthorizedRequest
    {
        [JsonProperty(propertyName: "salespageid")]
        public int SalesPageId { get; set; }

        [JsonProperty(propertyName: "agentpid")]
        public string AgentPId { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "validateaffiliate";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}