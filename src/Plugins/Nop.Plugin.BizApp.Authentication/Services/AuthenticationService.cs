using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Core.Infrastructure.Mapper;
using Nop.Core.Infrastructure;
using Nop.Plugin.BizApp.Core;
using Nop.Plugin.BizApp.Core.Domain.Api.Auth;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Plugin.BizApp.Core.Security;
using Microsoft.Extensions.Azure;
using DocumentFormat.OpenXml.EMMA;
using Nop.Plugin.BizApp.Authentication.Helpers;

namespace Nop.Plugin.BizApp.Authentication.Services
{
    public class AuthenticationService
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        private static Random random = new Random();

        public AuthenticationService(IGenericAttributeService genericAttributeService,
            BizAppHttpClient bizAppHttpClient,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IWorkContext workContext,
            CustomerSettings customerSettings,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IPermissionService permissionService)
        {
            _genericAttributeService = genericAttributeService;
            _bizAppHttpClient = bizAppHttpClient;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _workContext = workContext;
            _customerSettings = customerSettings;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Methods

        public async Task<CustomerLoginResults> ValidateCustomerAsync(string username, string password, string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return await _customerRegistrationService.ValidateCustomerAsync(username, password);

            try
            {
                var tokenResponse = await _bizAppHttpClient.RequestAsync<GetTokenRequest, TokenResponse>(new GetTokenRequest()
                {
                    Username = username,
                    Password = password,
                    Domain = domain
                });

                if (!tokenResponse.Success)
                    return CustomerLoginResults.WrongPassword;

                var token = tokenResponse.Token.First();
                var store = await _storeContext.GetCurrentStoreAsync();

                var customer = await _customerService.GetCustomerByUsernameAsync(UsernameHelper.CombineUsernameAndDomain(username, domain));
                if (customer == null)
                {
                    customer = await _workContext.GetCurrentCustomerAsync();

                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenAttribute, token.AccessToken);
                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenExpiryAttribute, token.Expire);
                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppRefreshTokenAttribute, token.RefreshToken);

                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        UsernameHelper.CombineUsernameAndDomain(username, domain) + "@bizapp.com",
                        UsernameHelper.CombineUsernameAndDomain(username, domain),
                        RandomString(20),
                        _customerSettings.DefaultPasswordFormat,
                        store.Id,
                        true);

                    var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                    if (registrationResult.Success)
                    {
                        var userInfoResponse = await _bizAppHttpClient.RequestAsync<GetUserInfoRequest, UserInfoResponse>(new GetUserInfoRequest()
                        {
                            Customer = customer
                        });

                        if (userInfoResponse.Success)
                        {
                            var userInfo = userInfoResponse.UserInfo.First();
                            if (_customerSettings.FirstNameEnabled)
                                customer.FirstName = userInfo.FullName;
                            if (_customerSettings.PhoneEnabled)
                                customer.Phone = userInfo.PhoneNumber;

                            await _customerService.UpdateCustomerAsync(customer);
                        }

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                    }
                }
                else
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenAttribute, token.AccessToken);
                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppTokenExpiryAttribute, token.Expire);
                    await _genericAttributeService.SaveAttributeAsync(customer, BizAppDefaults.BizAppRefreshTokenAttribute, token.RefreshToken);
                }

                return CustomerLoginResults.Successful;
            }
            catch (Exception ex)
            {
                return CustomerLoginResults.WrongPassword;
            }
        }


        #endregion
    }
}