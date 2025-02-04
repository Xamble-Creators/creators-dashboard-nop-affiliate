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

namespace Nop.Plugin.BizApp.Authentication.Helpers
{
    public class UsernameHelper
    {
        #region Methods

        public static string CombineUsernameAndDomain(string username, string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return username;

            return username + "-" + domain;
        }


        #endregion
    }
}