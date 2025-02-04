using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.BizApp.Core.Domain.Api.Auth
{
    /// <summary>
    /// Represents request to get access token
    /// </summary>
    public class RenewTokenRequest : ApiRequest
    {
        [JsonProperty(propertyName: "accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty(propertyName: "refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "authrenewtoken";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}