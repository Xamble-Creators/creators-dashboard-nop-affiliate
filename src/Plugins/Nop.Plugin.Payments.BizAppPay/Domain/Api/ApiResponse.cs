using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    /// <summary>
    /// Represents base response details
    /// </summary>
    public abstract class ApiResponse : IApiResponse
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }
    }
}