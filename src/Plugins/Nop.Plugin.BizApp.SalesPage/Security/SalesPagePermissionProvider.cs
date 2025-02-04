using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.BizApp.SalesPage.Security
{
    public partial class SalesPagePermissionProvider : IPermissionProvider
    {
        //admin area permissions
        public static readonly PermissionRecord ManageBizAppSalesPage = new() { Name = "Manage BizApp Sales Page", SystemName = "ManageBizAppSalesPage", Category = "BizApp" };
        public static readonly PermissionRecord ManageBizAppUsers = new() { Name = "Manage BizApp Users", SystemName = "ManageBizAppUsers", Category = "BizApp" };

        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageBizAppSalesPage,
                ManageBizAppUsers
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    SalesPageDefaults.BizAppUsersRoleName,
                    new[]
                    {
                        ManageBizAppSalesPage
                    }
                ),
                (
                    SalesPageDefaults.BizAppAdminsRoleName,
                    new[]
                    {
                        ManageBizAppSalesPage,
                        ManageBizAppUsers
                    }
                )
            };
        }
    }
}