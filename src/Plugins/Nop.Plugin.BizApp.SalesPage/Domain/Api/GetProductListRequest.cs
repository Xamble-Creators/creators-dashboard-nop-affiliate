using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class GetProductListRequest : ApiRequest, IAuthorizedRequest
    {
        [JsonProperty(propertyName: "agentpid")]
        public string AgentPId { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "getlistofproducts";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}