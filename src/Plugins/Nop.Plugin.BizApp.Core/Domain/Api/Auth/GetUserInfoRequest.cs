using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.BizApp.Core.Domain.Api.Auth
{
    public class GetUserInfoRequest : ApiRequest, IAuthorizedRequest
    {
        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "getuserinfo";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}