using Nop.Core.Domain.Customers;

namespace Nop.Plugin.BizApp.Core.Domain.Api
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
        /// Gets the request user
        /// </summary>
        public Customer Customer { get; set; }
    }
}