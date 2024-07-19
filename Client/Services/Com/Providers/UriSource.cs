using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Provider;

using Shared;

namespace CloudSyncDriveClient.Services.Com.Providers
{
    [ComVisible(true), Guid("97961bcb-601c-4950-927c-43b9319c7217")]
    internal class UriSource : IStorageProviderUriSource
    {
        public void GetContentInfoForPath(string path, StorageProviderGetContentInfoForPathResult result)
        {
            result.Status = StorageProviderUriSourceStatus.FileNotFound;

            var fileName = Path.GetFileName(path);
            result.ContentId = "http://cloudmirror.example.com/contentId/" + fileName;
            result.ContentUri = "http://cloudmirror.example.com/contentUri/" + fileName + "?StorageProviderId=TestStorageProvider";
            result.Status = StorageProviderUriSourceStatus.Success;
        }

        public void GetPathForContentUri(string contentUri, StorageProviderGetPathForContentUriResult result)
        {
            result.Status = StorageProviderUriSourceStatus.FileNotFound;

            const string prefix = "http://cloudmirror.example.com/contentUri/";
            var uri = contentUri;
            if (uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var localPath = Constants.Dummy.SyncRoot + "\\" + uri[prefix.Length..uri.IndexOf('?')];

                if (File.Exists(localPath))
                {
                    result.Path = localPath;
                    result.Status = StorageProviderUriSourceStatus.Success;
                }
            }
        }
    }
}
