using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.BizApp.Authentication.Models
{
    public partial record LoginModel : BaseNopModel
    {
        [NopResourceDisplayName("Account.Login.Fields.Username")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.Authentication.Domain")]
        public string Domain { get; set; }
    }
}