using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.Core.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.BizApp.Core.BizAppApiUrl")]
        public string BizAppApiUrl { get; set; }

        #endregion
    }
}