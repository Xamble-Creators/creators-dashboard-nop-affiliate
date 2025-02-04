using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Http;
using Nop.Data;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public partial class GenericRouteEventConsumer : IConsumer<GenericRoutingEvent>
    {
        public virtual async Task HandleEventAsync(GenericRoutingEvent eventMessage)
        {
            if (eventMessage.UrlRecord.EntityName == nameof(SalesPageRecord))
            {
                eventMessage.RouteValues[NopRoutingDefaults.RouteValue.Controller] = "SalesPageOrder";
                eventMessage.RouteValues[NopRoutingDefaults.RouteValue.Action] = "Order";

                if (eventMessage.HttpContext.Request.Path.HasValue)
                {
                    var path = eventMessage.HttpContext.Request.Path.ToString().Split("/", System.StringSplitOptions.RemoveEmptyEntries);
                    if (path.Length >= 2)
                        eventMessage.RouteValues[SalesPageDefaults.RouteAgentPId] = path[1];
                }
                eventMessage.RouteValues[SalesPageDefaults.RouteId] = eventMessage.UrlRecord.EntityId;
            }
        }
    }
}