using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.BizApp.Core.Domain.Api;
using Nop.Plugin.BizApp.Core.Domain.Api.Auth;
using Nop.Services.Common;
using Nop.Services.Logging;

namespace Nop.Plugin.BizApp.Core.Services
{
    /// <summary>
    /// Represents HTTP client to request third-party services
    /// </summary>
    public class BizAppHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly BizAppSettings _bizAppSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;

        private string _accessToken;

        #endregion

        #region Ctor

        public BizAppHttpClient(HttpClient httpClient,
            BizAppSettings bizAppSettings,
            IGenericAttributeService genericAttributeService,
            ILogger logger)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(BizAppDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = httpClient;
            _bizAppSettings = bizAppSettings;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get access token
        /// </summary>
        /// <returns>The asynchronous task whose result contains access token</returns>
        private async Task<string> GetAccessTokenAsync(Customer customer)
        {
            if (!string.IsNullOrEmpty(_accessToken))
                return _accessToken;

            if (string.IsNullOrEmpty(_bizAppSettings.BizAppApiUrl))
                throw new NopException("Api Url is not set");

            var tokenExpiry = await _genericAttributeService.GetAttributeAsync<long>(customer, BizAppDefaults.BizAppTokenExpiryAttribute);
            //we renew one day before
            if (DateTimeOffset.FromUnixTimeSeconds(tokenExpiry).AddDays(-1) >= DateTime.UtcNow)
            {
                _accessToken = await _genericAttributeService.GetAttributeAsync<string>(customer, BizAppDefaults.BizAppTokenAttribute);
                return _accessToken;
            }

            var newToken = (await RequestAsync<RenewTokenRequest, TokenResponse>(new()
            {
                AccessToken = await _genericAttributeService.GetAttributeAsync<string>(customer, BizAppDefaults.BizAppTokenAttribute),
                RefreshToken = await _genericAttributeService.GetAttributeAsync<string>(customer, BizAppDefaults.BizAppRefreshTokenAttribute)
            }))?.Token?.First();

            await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenAttribute, newToken.AccessToken);
            await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenExpiryAttribute, newToken.Expire);
            await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppRefreshTokenAttribute, newToken.RefreshToken);

            return _accessToken;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Request services
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request) where TRequest : IApiRequest where TResponse : IApiResponse
        {
            //prepare request parameters
            var requestString = JsonConvert.SerializeObject(request);
            var requestContent = (ByteArrayContent)new StringContent(requestString, Encoding.UTF8, MimeTypes.ApplicationJson);

            var uri = new Uri(new Uri(_bizAppSettings.BizAppApiUrl), request.Path.TrimEnd('/') + '/');
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), uri)
            {
                Content = requestContent
            };

            //add authorization
            if (request is IAuthorizedRequest)
            {
                var accessToken = await GetAccessTokenAsync(request.Customer);
                requestMessage.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");
            }

            if (!(request is GetTokenRequest))
                await _logger.InformationAsync($"Calling {uri.ToString()} with body : {JsonConvert.SerializeObject(request)}...");

            //execute request and get result
            var httpResponse = await _httpClient.SendAsync(requestMessage);
            var responseString = await httpResponse.Content.ReadAsStringAsync();

            if (!(request is GetTokenRequest))
                await _logger.InformationAsync($"Called {uri.ToString()} with response body : {responseString}.");

            var result = JsonConvert.DeserializeObject<TResponse>(responseString ?? string.Empty);
            if (!result?.Success ?? false)
                throw new NopException($"Request error: {result.ErrorMessage}");

            return result;
        }

        #endregion
    }
}