using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder
{
    public record PostageModel : BaseNopModel
    {
        #region Properties

        public IList<int> AllowedStateIds { get; set; }

        public int MinimumWeight { get; set; }

        public int MaximumWeight { get; set; }

        public decimal Price { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        #endregion
    }
}