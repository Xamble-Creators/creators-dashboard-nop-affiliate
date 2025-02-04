using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageDashboard
{
    public record DashboardModel : BaseNopModel
    {
        public DashboardModel()
        {
            AvailableCustomers = new List<SelectListItem>();
            AvailableSalesPages = new List<SelectListItem>();
        }

        #region Properties

        public IList<SelectListItem> AvailableCustomers { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.User")]
        public int? SelectedCustomerId { get; set; }

        public IList<SelectListItem> AvailableSalesPages { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.SalesPage")]
        public int? SelectedSalesPageId { get; set; }

        #endregion
    }
}