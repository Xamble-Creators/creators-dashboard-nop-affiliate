using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    /// <summary>
    /// Represents request to get access token
    /// </summary>
    public class GetTokenRequest : ApiRequest
    {
        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "api/v3/token";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}