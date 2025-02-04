using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Media;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.BizApp.SalesPage.Security;
using System.Linq;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.Core;

namespace Nop.Plugin.BizApp.SalesPage.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    //Controller for Roxy fileman (http://www.roxyfileman.com/) for TinyMCE editor
    //the original file was \RoxyFileman-1.4.5-net\fileman\asp_net\main.ashx

    //do not validate request token (XSRF)
    public partial class SalesPageRoxyFilemanController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly SalesPageRoxyFilemanService _roxyFilemanService;
        private readonly SalesPageRecordService _salesPageRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly SalesPageSettings _salesPageSettings;

        #endregion

        #region Ctor

        public SalesPageRoxyFilemanController(IPermissionService permissionService,
            SalesPageRoxyFilemanService roxyFilemanService,
            SalesPageRecordService salesPageRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            SalesPageSettings salesPageSettings)
        {
            _permissionService = permissionService;
            _roxyFilemanService = roxyFilemanService;
            _salesPageRecordService = salesPageRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _salesPageSettings = salesPageSettings;
        }

        #endregion

        #region Utils

        protected virtual IEnumerable<object> AllFilesForSalesPageRecord(string imageKey)
        {
            var directories = _roxyFilemanService.GetDirectoryList(imageKey, "image");
            return directories.SelectMany(d => _roxyFilemanService.GetFiles(d.GetType().GetProperty("p").GetValue(d, null).ToString(), "image"));
        }

        protected virtual JsonResult JsonOk()
        {
            return Json(new { res = "ok" });
        }

        protected virtual JsonResult JsonError(string message)
        {
            return Json(new { res = "error", msg = message });
        }

        #endregion

        #region Methods

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> DirectoriesList(string imageKey, string type)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                var directories = _roxyFilemanService.GetDirectoryList(imageKey, type);
                return Json(directories);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> FilesList(string imageKey, string d, string type)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                var directories = _roxyFilemanService.GetFiles(d, type);

                return Json(directories);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public virtual async Task<IActionResult> DownloadFile(string imageKey, string f)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                var (stream, name) = _roxyFilemanService.GetFileStream(f);

                if (!new FileExtensionContentTypeProvider().TryGetContentType(f, out var contentType))
                    contentType = MimeTypes.ApplicationOctetStream;

                return File(stream, contentType, name);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> CopyDirectory(string imageKey, string d, string n)
        {
            return JsonError("Copy Directory is disabled");
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> CopyFile(string imageKey, string f, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                if (AllFilesForSalesPageRecord(imageKey).Count() >= _salesPageSettings.AllowedFileCountPerSalesPage)
                    throw new Exception($"You can't have more than {_salesPageSettings.AllowedFileCountPerSalesPage} files per sales page");

                _roxyFilemanService.CopyFile(f, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> CreateDirectory(string imageKey, string d, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.CreateDirectory(d, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> DeleteDirectory(string imageKey, string d)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.DeleteDirectory(d);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> DeleteFile(string imageKey, string f)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.DeleteFile(f);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public virtual async Task<IActionResult> DownloadDirectory(string imageKey, string d)
        {
            var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
            if (salesPageRecord != null)
            {
                if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                {
                    if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                        throw new Exception("You don't have required permission");
                }
            }

            var fileContents = _roxyFilemanService.DownloadDirectory(d);

            return File(fileContents, MimeTypes.ApplicationZip);
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> MoveDirectory(string imageKey, string d, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.MoveDirectory(d, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> MoveFile(string imageKey, string f, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.MoveFile(f, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> RenameDirectory(string imageKey, string d, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.RenameDirectory(d, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> RenameFile(string imageKey, string f, string n)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                _roxyFilemanService.RenameFile(f, n);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public virtual async Task<IActionResult> CreateImageThumbnail(string imageKey, string f)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(f, out var contentType))
                    contentType = MimeTypes.ImageJpeg;

                var fileContents = _roxyFilemanService.CreateImageThumbnail(f, contentType);

                return File(fileContents, contentType);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> UploadFiles(string imageKey, [FromForm] RoxyFilemanUploadModel uploadModel)
        {
            try
            {
                var salesPageRecord = await _salesPageRecordService.GetRecordByImageKeyAsync(imageKey);
                if (salesPageRecord != null)
                {
                    if (!await _permissionService.AuthorizeAsync(SalesPagePermissionProvider.ManageBizAppUsers))
                    {
                        if (salesPageRecord.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                            throw new Exception("You don't have required permission");
                    }
                }

                if (HttpContext.Request.Form.Files.Count == 0)
                    throw new RoxyFilemanException("E_UploadNoFiles");

                if (AllFilesForSalesPageRecord(imageKey).Count() >= _salesPageSettings.AllowedFileCountPerSalesPage)
                    throw new Exception($"You can't have more than {_salesPageSettings.AllowedFileCountPerSalesPage} files per sales page");

                foreach (var file in HttpContext.Request.Form.Files)
                {
                    if (file.Length > _salesPageSettings.FileSizeLimitInBytes)
                        throw new Exception($"File size cannot be more than {_salesPageSettings.FileSizeLimitInBytes / 1000 / 1000} MB per file");
                }

                await _roxyFilemanService.UploadFilesAsync(uploadModel.D, HttpContext.Request.Form.Files);

                return JsonOk();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        #endregion

    }
}