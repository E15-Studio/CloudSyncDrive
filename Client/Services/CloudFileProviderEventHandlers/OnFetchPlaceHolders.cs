using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.CldApi;
using Vanara.Extensions;
using Vanara.PInvoke;
using Vanara.InteropServices;
using static Vanara.PInvoke.Kernel32;
using System.IO;
using Windows.Storage;

namespace CloudSyncDriveClient.Services.CloudFileProviderEventHandlers
{
    internal static class OnFetchPlaceHolders
    {
        public static void Handler(in CF_CALLBACK_INFO callbackInfo, in CF_CALLBACK_PARAMETERS param)
        {
            lock (CloudFileProvider.GetSyncRootLock())
            {
                var filename = StringHelper.GetString(callbackInfo.FileIdentity, CharSet.Unicode);
                if (filename == null)
                    filename = "";

                var opInfo = new CF_OPERATION_INFO()
                {
                    StructSize = (uint)Marshal.SizeOf<CF_OPERATION_INFO>(),
                    Type = CF_OPERATION_TYPE.CF_OPERATION_TYPE_TRANSFER_PLACEHOLDERS,
                    ConnectionKey = callbackInfo.ConnectionKey,
                    TransferKey = callbackInfo.TransferKey,
                    RequestKey = callbackInfo.RequestKey,
                    SyncStatus = CF_SYNC_PROVIDER_STATUS.CF_PROVIDER_STATUS_SYNC_FULL.MarshalToPtr(Marshal.AllocHGlobal, out _)
                };

                var placeHolders = GetPlaceHoldersForPath(filename);

                IntPtr rawPlaceholders = default;
                int count = 0;

                var placeholders = GetPlaceHoldersForPath(filename);
                count = placeHolders.Length;

                if (count != 0)
                    rawPlaceholders = placeholders.MarshalToPtr(Marshal.AllocHGlobal, out _);

                var opParams = new CF_OPERATION_PARAMETERS
                {
                    ParamSize = (uint)Marshal.SizeOf<CF_OPERATION_PARAMETERS>(),
                    TransferPlaceholders = new CF_OPERATION_PARAMETERS.TRANSFERPLACEHOLDERS()
                    {
                        PlaceholderArray = rawPlaceholders,
                        PlaceholderCount = (uint)count,
                        PlaceholderTotalCount = (uint)count,
                        EntriesProcessed = 0,
                        Flags = CF_OPERATION_TRANSFER_PLACEHOLDERS_FLAGS.CF_OPERATION_TRANSFER_PLACEHOLDERS_FLAG_NONE,
                        CompletionStatus = HRESULT.S_OK
                    }
                };

                var hresult = CfExecute(opInfo, ref opParams);

                if (count != 0)
                    Marshal.FreeHGlobal(rawPlaceholders);

                if (hresult != HRESULT.S_OK)
                {
                    Console.Write($"Failed with error {hresult}");
                    hresult.ThrowIfFailed($"Failed with error {hresult}");
                }

                Console.WriteLine($"Fetched placeholders for '{filename}'");
            }
        }

        private static CF_PLACEHOLDER_CREATE_INFO[] GetPlaceHoldersForPath(string path)
        {
            var di = SyncEngine.Directory.GetRemote(path);

            List<CF_PLACEHOLDER_CREATE_INFO> placeHolders = new();

            foreach (var file in di.GetFiles())
            {
                var fileFullPath = Path.Join(path, file.Name);
                var pIdentity = new SafeCoTaskMemString(fileFullPath);

                FILE_BASIC_INFO metadata = new FILE_BASIC_INFO()
                {
                    FileAttributes = (FileFlagsAndAttributes)file.Attributes,
                    CreationTime = file.CreationTime.ToFileTimeStruct(),
                    LastWriteTime = file.LastWriteTime.ToFileTimeStruct(),
                    LastAccessTime = file.LastAccessTime.ToFileTimeStruct(),
                    ChangeTime = file.LastWriteTime.ToFileTimeStruct()
                };

                var placeHolder = new CF_PLACEHOLDER_CREATE_INFO()
                {
                    FileIdentity = pIdentity,
                    FileIdentityLength = (uint)pIdentity.Size,
                    RelativeFileName = file.Name,
                    Flags = CF_PLACEHOLDER_CREATE_FLAGS.CF_PLACEHOLDER_CREATE_FLAG_MARK_IN_SYNC,
                    FsMetadata = new CF_FS_METADATA
                    {
                        FileSize = file.Length,
                        BasicInfo = metadata
                    }
                };

                placeHolders.Add(placeHolder);
            }

            foreach (var dir in di.GetSubDirectories())
            {
                var fileFullPath = Path.Join(path, dir.Name);
                var pIdentity = new SafeCoTaskMemString(fileFullPath);

                FILE_BASIC_INFO metadata = new FILE_BASIC_INFO()
                {
                    FileAttributes = (FileFlagsAndAttributes)dir.Attributes,
                    CreationTime = dir.CreationTime.ToFileTimeStruct(),
                    LastWriteTime = dir.LastWriteTime.ToFileTimeStruct(),
                    LastAccessTime = dir.LastAccessTime.ToFileTimeStruct(),
                    ChangeTime = dir.LastWriteTime.ToFileTimeStruct()
                };

                var placeHolder = new CF_PLACEHOLDER_CREATE_INFO()
                {
                    FileIdentity = pIdentity,
                    FileIdentityLength = (uint)pIdentity.Size,
                    RelativeFileName = dir.Name,
                    Flags = CF_PLACEHOLDER_CREATE_FLAGS.CF_PLACEHOLDER_CREATE_FLAG_MARK_IN_SYNC,
                    FsMetadata = new CF_FS_METADATA
                    {
                        FileSize = 0,
                        BasicInfo = metadata
                    }
                };

                placeHolders.Add(placeHolder);
            }

            return placeHolders.ToArray();
        }
    }
}
