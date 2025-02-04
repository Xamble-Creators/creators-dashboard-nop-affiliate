using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class GetPostageListRequest : ApiRequest, IAuthorizedRequest
    {
        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "getlistofpostage";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}