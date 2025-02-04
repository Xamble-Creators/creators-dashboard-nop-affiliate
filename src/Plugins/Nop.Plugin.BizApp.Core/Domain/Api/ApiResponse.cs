using Newtonsoft.Json;

namespace Nop.Plugin.BizApp.Core.Domain.Api
{
    /// <summary>
    /// Represents base response details
    /// </summary>
    public abstract class ApiResponse : IApiResponse
    {
        [JsonProperty(PropertyName = "errormessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
    }
}