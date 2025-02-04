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
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public partial class CustomerLoggedinEventConsumer : IConsumer<CustomerLoggedinEvent>
    {
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger<CustomerLoggedinEventConsumer> _logger;

        public CustomerLoggedinEventConsumer(BizAppHttpClient bizAppHttpClient,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILogger<CustomerLoggedinEventConsumer> logger)
        {
            _bizAppHttpClient = bizAppHttpClient;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
        }

        public virtual async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            try
            {
                var userInfoResponse = await _bizAppHttpClient.RequestAsync<GetUserInfoRequest, UserInfoResponse>(new GetUserInfoRequest()
                {
                    Customer = eventMessage.Customer
                });

                if (userInfoResponse.Success)
                {
                    await _genericAttributeService.SaveAttributeAsync(eventMessage.Customer, SalesPageDefaults.BizAppSalesPageLimitAttribute,
                        userInfoResponse.UserInfo.First().MaxSp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update user sales page limit for {eventMessage.Customer.Username}");
            }
        }
    }
}