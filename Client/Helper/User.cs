using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Shared;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace CloudSyncDriveClient.Helper
{
    internal class User
    {
        /// <summary>
        /// Get the SID of the current user
        /// </summary>
        /// <returns>SID string looks like "x-x-xx-xxx-xxxxx"</returns>
        /// <exception cref="Exception"></exception>
        public static string GetCurrentUserSID()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            var identity = currentUser.User;
            if (identity == null)
                throw new Exception("User not found");

            return identity.ToString();
        }

        /// <summary>
        /// Get the sync root id
        /// </summary>
        /// <see cref="https://learn.microsoft.com/en-us/uwp/api/windows.storage.provider.storageprovidersyncrootinfo.id?view=winrt-26100#windows-storage-provider-storageprovidersyncrootinfo-id"/>
        /// <returns></returns>
        public static string GetSyncRootId()
        {
            return $"{Constants.Dummy.StorageProviderId}!" +
                $"{GetCurrentUserSID()}!" +
                $"{Constants.Dummy.AccountId}";
        }


        public static Uri GetRecycleBinWebUIUrl()
        {
            var url = Constants.Dummy.RecycleBinWebUIUrl;
            return new Uri(url);
        }

        public static IBuffer GetSyncRootIdentityContext(string mountPoint)
        {
            var syncRootIdentity = $"{mountPoint}->{Constants.Dummy.SyncSource}";
            return CryptographicBuffer.ConvertStringToBinary(syncRootIdentity, BinaryStringEncoding.Utf8);
        }
    }
}
