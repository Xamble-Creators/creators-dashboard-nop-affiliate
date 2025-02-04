using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Http;
using Nop.Data;
using Nop.Plugin.BizApp.Core.Domain.Api.Auth;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public partial class CustomerRegisteredEventConsumer : IConsumer<CustomerRegisteredEvent>
    {
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerRegisteredEventConsumer> _logger;

        public CustomerRegisteredEventConsumer(BizAppHttpClient bizAppHttpClient,
            ICustomerService customerService,
            ILogger<CustomerRegisteredEventConsumer> logger)
        {
            _bizAppHttpClient = bizAppHttpClient;
            _customerService = customerService;
            _logger = logger;
        }

        public virtual async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            try
            {
                var userInfoResponse = await _bizAppHttpClient.RequestAsync<GetUserInfoRequest, UserInfoResponse>(new GetUserInfoRequest()
                {
                    Customer = eventMessage.Customer
                });

                if (userInfoResponse.Success)
                {
                    var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(SalesPageDefaults.BizAppUsersRoleName);

                    //add biz app sales customer role if this user is bizapp user
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping()
                    {
                        CustomerId = eventMessage.Customer.Id,
                        CustomerRoleId = customerRole.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update user role for {eventMessage.Customer.Username}");
            }
        }
    }
}