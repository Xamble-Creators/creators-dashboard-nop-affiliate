using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.BizApp.SalesPage.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var genericCatalogPattern = $"{{{NopRoutingDefaults.RouteValue.SeName}}}/{{{SalesPageDefaults.RouteAgentPId}}}";
            endpointRouteBuilder.MapDynamicControllerRoute<SlugRouteTransformer>(genericCatalogPattern);

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageThankYou",
                pattern: "salespage/thankyou/{customOrderNumber}",
                defaults: new { controller = "SalesPageOrder", action = "ThankYou" });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageNotFound",
                pattern: "page-not-found",
                defaults: new { controller = "SalesPageOrder", action = "SalesPageNotFound" });

            #region Custom Error

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageError",
                pattern: "Error/Error",
                defaults: new { controller = "SalesPageError", action = "Error" });

            #endregion


            #region Roxy Fileman

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanDirectoriesList",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/DirectoriesList",
                defaults: new { controller = "SalesPageRoxyFileman", action = "DirectoriesList", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanFilesList",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/FilesList",
                defaults: new { controller = "SalesPageRoxyFileman", action = "FilesList", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanDownloadFile",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/DownloadFile",
                defaults: new { controller = "SalesPageRoxyFileman", action = "DownloadFile", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanCopyDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/CopyDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "CopyDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanCopyFile",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/CopyFile",
                defaults: new { controller = "SalesPageRoxyFileman", action = "CopyFile", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanCreateDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/CreateDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "CreateDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanDeleteDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/DeleteDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "DeleteDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanDeleteFile",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/DeleteFile",
                defaults: new { controller = "SalesPageRoxyFileman", action = "DeleteFile", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanDownloadDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/DownloadDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "DownloadDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanMoveDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/MoveDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "MoveDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanMoveFile",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/MoveFile",
                defaults: new { controller = "SalesPageRoxyFileman", action = "MoveFile", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanRenameDirectory",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/RenameDirectory",
                defaults: new { controller = "SalesPageRoxyFileman", action = "RenameDirectory", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanRenameFile",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/RenameFile",
                defaults: new { controller = "SalesPageRoxyFileman", action = "RenameFile", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanCreateImageThumbnail",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/CreateImageThumbnail",
                defaults: new { controller = "SalesPageRoxyFileman", action = "CreateImageThumbnail", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "SalesPageRoxyFilemanUploadFiles",
                pattern: "Admin/SalesPageRoxyFileman/{imageKey}/UploadFiles",
                defaults: new { controller = "SalesPageRoxyFileman", action = "UploadFiles", area = AreaNames.Admin });

            #endregion
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1;
    }
}