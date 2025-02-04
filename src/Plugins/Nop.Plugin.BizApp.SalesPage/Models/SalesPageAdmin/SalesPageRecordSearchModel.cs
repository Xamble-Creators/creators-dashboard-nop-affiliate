using System.Collections;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin
{
    public record SalesPageRecordSearchModel : BaseSearchModel
    {
        public SalesPageRecordSearchModel()
        {
            AvailableCustomers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.Users")]
        public int? CustomerId { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.PageName")]
        public string SearchPageName { get; set; }
    }
}