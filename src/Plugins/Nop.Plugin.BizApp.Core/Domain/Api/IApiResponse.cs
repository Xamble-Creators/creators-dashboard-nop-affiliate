namespace Nop.Plugin.BizApp.Core.Domain.Api
{
    /// <summary>
    /// Represents response from the service
    /// </summary>
    public interface IApiResponse
    {
        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }
}