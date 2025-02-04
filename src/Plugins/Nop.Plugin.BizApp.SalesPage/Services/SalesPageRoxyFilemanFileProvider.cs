using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using SkiaSharp;

namespace Nop.Services.Media.RoxyFileman
{
    public class SalesPageRoxyFilemanFileProvider : RoxyFilemanFileProvider
    {
        #region Fields

        protected INopFileProvider _nopFileProvider;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public SalesPageRoxyFilemanFileProvider(INopFileProvider nopFileProvider): base(nopFileProvider)
        {
            _nopFileProvider = nopFileProvider;
        }

        public SalesPageRoxyFilemanFileProvider(INopFileProvider defaultFileProvider, MediaSettings mediaSettings) : base(defaultFileProvider, mediaSettings)
        {
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utils

        #endregion

        #region Methods

        /// <summary>
        /// Rename the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="newName">New name of the directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual void RenameDirectory(string sourcePath, string newName)
        {
            try
            {
                var destDirName = Path.Combine(Path.GetDirectoryName(sourcePath), newName);
                var sourceDirName = sourcePath;

                if (destDirName.IndexOf(sourceDirName, StringComparison.InvariantCulture) == 0)
                    throw new RoxyFilemanException("E_CannotMoveDirToChild");

                var sourceDirInfo = new DirectoryInfo(GetFullPath(sourceDirName));
                if (!sourceDirInfo.Exists)
                    throw new RoxyFilemanException("E_MoveDirInvalisPath");

                if (string.Equals(sourceDirInfo.FullName, Root, StringComparison.InvariantCultureIgnoreCase))
                    throw new RoxyFilemanException("E_MoveDir");

                var destinationDirInfo = new DirectoryInfo(GetFullPath(destDirName));
                if (destinationDirInfo.Exists)
                    throw new RoxyFilemanException("E_DirAlreadyExists");

                try
                {
                    sourceDirInfo.MoveTo(destinationDirInfo.FullName);
                }
                catch
                {
                    throw new RoxyFilemanException("E_MoveDir");
                }
            }
            catch (Exception ex)
            {
                throw new RoxyFilemanException("E_RenameDir", ex);
            }
        }

        #endregion
    }
}