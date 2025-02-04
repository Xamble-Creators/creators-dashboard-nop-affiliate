using DocumentFormat.OpenXml.Spreadsheet;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.BizApp.SalesPage.Factories;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Plugin.BizApp.SalesPage.Services;
using Nop.Plugin.BizApp.SalesPage.Validators;
using Nop.Services.Media.RoxyFileman;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.BizApp.SalesPage.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<SalesPageRecordModelFactory>();
            services.AddScoped<SalesPageOrderModelFactory>();
            services.AddScoped<SalesPageRecordService>();
            services.AddScoped<SalesPageOrderService>();
            services.AddScoped<SalesPageProductService>();
            services.AddScoped<SalesPageReportService>();
            services.AddScoped<SalesPageVisitService>();
            services.AddScoped<SalesPageRoxyFilemanService>();
            services.AddScoped<SalesPageRoxyFilemanFileProvider>();
            services.AddScoped<SalesPageOrderAdminModelFactory>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 3000;
    }
}