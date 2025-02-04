using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class PostageListResponse : ApiResponse
    {
        #region Properties

        public IList<PostageData> PostageList { get; set; }

        #endregion

        #region Nested classes

        public class PostageData
        {
            public string Id { get; set; }
            public string PostageName { get; set; }

            public int MinimumWeight { get; set; }

            public int MaximumWeight { get; set; }

            public int ZoneSm { get; set; }

            public int ZoneSs { get; set; }

            public int ZoneSrwk { get; set; }

            public int ZoneOthers { get; set; }

            public decimal Price { get; set; }
        }

        #endregion
    }
}