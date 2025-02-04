using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Services;

namespace Nop.Services.Media.RoxyFileman
{
    public class SalesPageRoxyFilemanService : RoxyFilemanService
    {
        #region Fields

        private readonly SalesPageRoxyFilemanFileProvider _fileProvider;

        #endregion

        #region Ctor

        public SalesPageRoxyFilemanService(
            SalesPageRoxyFilemanFileProvider fileProvider,
            IWorkContext workContext) : base(fileProvider, workContext)
        {
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utils

        #endregion

        #region Directories

        /// <summary>
        /// Get all available directories as a directory tree
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns>List of directories</returns>
        public IEnumerable<object> GetDirectoryList(string imageKey, string type)
        {
            var rootDirectoryPath = string.Format("salespage-{0}", imageKey);
            base.CreateDirectory(string.Empty, rootDirectoryPath);

            var contents = _fileProvider.GetDirectories(type, rootDirectoryPath: rootDirectoryPath);

            var result = new List<object>() { new
                {
                    p = rootDirectoryPath,
                    f = _fileProvider.GetFiles(rootDirectoryPath, type).Count(),
                    d = 0
                } };

            foreach (var (path, countFiles, countDirectories) in contents)
            {
                result.Add(new
                {
                    p = path.Replace("\\", "/"),
                    f = countFiles,
                    d = countDirectories
                });
            }

            return result;
        }

        /// <summary>
        /// Copy the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        public void CopyDirectory(string sourcePath, string destinationPath)
        {
            _fileProvider.CopyDirectory(sourcePath, destinationPath + "-Copy-1");
        }

        #endregion

        #region Files

        #endregion
    }
}