using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CloudSyncDriveClient.Helper;
using CloudSyncDriveClient.Services.Com;
using Shared;
using Windows.Storage;
using Windows.Storage.Provider;
using static Vanara.PInvoke.CldApi;
using static Vanara.PInvoke.Kernel32;

using CloudSyncDriveClient.Services.CloudFileProviderEventHandlers;

namespace CloudSyncDriveClient.Services
{
    internal static class CloudFileProvider
    {
        private static string? mountPoint { get; set; }
        private static CF_CONNECTION_KEY connectionKey;
        private static readonly object _lock = new();

        public static void Start(string mountPoint)
        {
            if (string.IsNullOrEmpty(mountPoint))
                throw new ArgumentNullException(nameof(mountPoint));

            if (!Directory.Exists(mountPoint))
                Directory.CreateDirectory(mountPoint);

            WindowsSearchManager.AddFolderToSearchIndexer(mountPoint);

            ComServiceManager.LaunchComServices();

            Mount(mountPoint);
        }

        public static void Stop()
        {
            if (mountPoint == null)
                return;

            lock (_lock)
            {
                Unmount();
                ComServiceManager.StopComServices();
            }
        }

        private static void Mount(string mountPoint)
        {
            if (!string.IsNullOrEmpty(CloudFileProvider.mountPoint))
                throw new Exception("Already mounted");

            CloudFileProvider.mountPoint = mountPoint;

            RegisterCloudFileProvicer();

            RegisterCloudFileEventHandlers();
        }

        private static void Unmount()
        {
            UnRegisterCloudFileEventHandlers();

            UnregisterCloudFileProvicer();
        }

        private static void RegisterCloudFileProvicer()
        {
            if (string.IsNullOrEmpty(mountPoint))
                throw new ArgumentNullException(nameof(mountPoint));

            StorageProviderSyncRootInfo info = new();
            info.Id = User.GetSyncRootId();
            info.Path = StorageFolder.GetFolderFromPathAsync(mountPoint).AsTask().Result;
            info.DisplayNameResource = Constants.Brand.SyncRootDisplayName;
            info.IconResource = Constants.Brand.SyncRootDisplayIcon;
            info.HydrationPolicy = StorageProviderHydrationPolicy.Full;
            info.HydrationPolicyModifier = StorageProviderHydrationPolicyModifier.None;
            info.PopulationPolicy = StorageProviderPopulationPolicy.Full;
            info.InSyncPolicy =
                StorageProviderInSyncPolicy.FileCreationTime
                | StorageProviderInSyncPolicy.DirectoryCreationTime;
            info.Version = Constants.Version;
            info.ShowSiblingsAsGroup = false;
            info.HardlinkPolicy = StorageProviderHardlinkPolicy.None;
            info.RecycleBinUri = User.GetRecycleBinWebUIUrl();
            info.Context = User.GetSyncRootIdentityContext(mountPoint);

            // TODO: add custom states here.
            // var customStates = info.StorageProviderItemPropertyDefinitions;
            // AddCustomState(customStates, "CustomStateName1", 1);
            // AddCustomState(customStates, "CustomStateName2", 2);
            // AddCustomState(customStates, "CustomStateName3", 3);

            StorageProviderSyncRootManager.Register(info);

            // Give the cache some time to invalidate
            Sleep(1000);
        }

        private static void UnregisterCloudFileProvicer()
        {
            if (mountPoint == null)
                throw new Exception("Mount point is not set");

            StorageProviderSyncRootManager.Unregister(User.GetSyncRootId());
        }

        private static void RegisterCloudFileEventHandlers()
        {
            if (mountPoint == null)
                throw new ArgumentNullException(nameof(mountPoint));

            var callbacks = new CF_CALLBACK_REGISTRATION[]
            {
                new CF_CALLBACK_REGISTRATION
                {
                    Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_FETCH_PLACEHOLDERS,
                    Callback = OnFetchPlaceHolders.Handler,
                },
                new CF_CALLBACK_REGISTRATION
                {
                    Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_FETCH_DATA,
                    Callback = CloudFileSyncHandlers.OnFetchData,
                },
                new CF_CALLBACK_REGISTRATION
                {
                    Type = CF_CALLBACK_TYPE.CF_CALLBACK_TYPE_CANCEL_FETCH_DATA,
                    Callback = CloudFileSyncHandlers.OnCancelFetchData,
                },
                CF_CALLBACK_REGISTRATION.CF_CALLBACK_REGISTRATION_END
            };

            CfConnectSyncRoot(mountPoint, callbacks, default,
                CF_CONNECT_FLAGS.CF_CONNECT_FLAG_REQUIRE_PROCESS_INFO |
                CF_CONNECT_FLAGS.CF_CONNECT_FLAG_REQUIRE_FULL_FILE_PATH,
                out connectionKey
                ).ThrowIfFailed();
        }

        private static void UnRegisterCloudFileEventHandlers()
        {
            CfDisconnectSyncRoot(connectionKey).ThrowIfFailed();
        }

        public static object GetSyncRootLock()
        {
            return _lock;
        }
    }
}
