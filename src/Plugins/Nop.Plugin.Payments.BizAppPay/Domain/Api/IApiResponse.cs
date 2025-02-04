using Newtonsoft.Json;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    /// <summary>
    /// Represents response from the service
    /// </summary>
    public interface IApiResponse
    {
        public string Status { get; set; }

        public string Message { get; set; }
    }
}