using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageError
{
    public record ErrorModel : BaseNopModel
    {
        public ErrorModel()
        {
        }

        #region Properties

        public string ErrorMessage { get; set; }

        #endregion
    }
}