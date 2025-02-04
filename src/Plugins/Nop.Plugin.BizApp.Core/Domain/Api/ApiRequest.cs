using Newtonsoft.Json;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.BizApp.Core.Domain.Api
{
    /// <summary>
    /// Represents base request object
    /// </summary>
    public abstract class ApiRequest : IApiRequest
    {
        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public abstract string Path { get; }

        /// <summary>
        /// Gets the request method
        /// </summary>
        [JsonIgnore]
        public abstract string Method { get; }

        /// <summary>
        /// Gets the request user
        /// </summary>
        [JsonIgnore]
        public Customer Customer { get; set; }
    }
}