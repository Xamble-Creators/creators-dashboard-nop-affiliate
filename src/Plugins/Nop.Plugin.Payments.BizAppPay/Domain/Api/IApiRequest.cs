using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    /// <summary>
    /// Represents request object
    /// </summary>
    public interface IApiRequest
    {
        /// <summary>
        /// Gets the request path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the request method
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the request apiKey
        /// </summary>
        public string ApiKey { get; set; }
    }
}