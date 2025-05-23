﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Media;
using Nop.Plugin.BizApp.Authentication.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.Authentication
{
    public class AuthenticationPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AuthenticationPlugin(IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IPermissionService permissionService)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.BizApp.Authentication.Domain"] = "Domain",
                ["Plugins.BizApp.Authentication.Domain.Required"] = "Domain is required",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.BizApp.Authentication");

            await base.UninstallAsync();
        }

        #endregion
    }
}