using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.BizApp.Core;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.Payments.BizAppPay.Domain.Api;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Seo;

namespace Nop.Plugin.Payments.BizAppPay.Services
{
    public partial class BizAppPayHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly BizAppPayPaymentSettings _bizAppPayPaymentSettings;
        private readonly ILogger _logger;

        private string _accessToken;

        #endregion

        #region Ctor

        public BizAppPayHttpClient(HttpClient httpClient,
            BizAppPayPaymentSettings bizAppPayPaymentSettings,
            ILogger logger)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(BizAppDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = httpClient;
            _bizAppPayPaymentSettings = bizAppPayPaymentSettings;
            _logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get access token
        /// </summary>
        /// <returns>The asynchronous task whose result contains access token</returns>
        private async Task<string> GetAccessTokenAsync(string apiKey)
        {
            if (!string.IsNullOrEmpty(_accessToken))
                return _accessToken;

            if (string.IsNullOrEmpty(_bizAppPayPaymentSettings.Endpoint))
                throw new NopException("Endpoint is not set");

            _accessToken = (await RequestAsync<GetTokenRequest, TokenResponse>(new()
            {
                ApiKey = apiKey
            }))?.Token;

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
            var requestContent = new MultipartFormDataContent();
            foreach (var prop in request.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var key = prop.Name;
                if (prop.GetCustomAttribute<JsonPropertyAttribute>() != null)
                    key = prop.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;

                var value = (string)prop.GetValue(request, null);
                if (value != null)
                    requestContent.Add(new StringContent((string)prop.GetValue(request, null)), key);
            }

            var header = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");
            requestContent.Headers.ContentDisposition = header;

            var uri = new Uri(new Uri(_bizAppPayPaymentSettings.Endpoint), request.Path.TrimEnd('/') + '/');
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), uri)
            {
                Content = requestContent
            };

            //add authorization
            if (!(request is GetTokenRequest))
            {
                var accessToken = await GetAccessTokenAsync(request.ApiKey);
                requestMessage.Headers.Add("Authentication", accessToken);
            }

            if (!(request is GetTokenRequest))
                await _logger.InformationAsync($"Calling {uri.ToString()} with body : {JsonConvert.SerializeObject(request)}...");

            //execute request and get result
            var httpResponse = await _httpClient.SendAsync(requestMessage);
            var responseString = await httpResponse.Content.ReadAsStringAsync();

            if (!(request is GetTokenRequest))
                await _logger.InformationAsync($"Called {uri.ToString()} with response body : {responseString}.");

            var result = JsonConvert.DeserializeObject<TResponse>(responseString ?? string.Empty);
            if (result?.Status?.ToLower() != "ok")
                throw new NopException($"Request error: {result.Message}");

            return result;
        }

        #endregion
    }
}