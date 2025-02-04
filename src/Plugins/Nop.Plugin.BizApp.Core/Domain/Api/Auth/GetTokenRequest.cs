using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.BizApp.Core.Domain.Api.Auth
{
    /// <summary>
    /// Represents request to get access token
    /// </summary>
    public class GetTokenRequest : ApiRequest
    {
        [JsonProperty(propertyName: "username")]
        public string Username { get; set; }

        [JsonProperty(propertyName: "password")]
        public string Password { get; set; }

        [JsonProperty(propertyName: "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "authtoken";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}