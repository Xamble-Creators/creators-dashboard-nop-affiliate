using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageError;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    //do not inherit it from BasePublicController. otherwise a lot of extra action filters will be called
    //they can create guest account(s), etc
    public partial class SalesPageErrorController : Controller
    {
        public virtual IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var model = new ErrorModel();
            if (!string.IsNullOrEmpty(exceptionHandlerPathFeature?.Error?.Message))
                model.ErrorMessage = exceptionHandlerPathFeature?.Error?.Message;

            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("~/Plugins/BizApp.SalesPage/Views/SalesPageError/Error.cshtml", model);
        }
    }
}