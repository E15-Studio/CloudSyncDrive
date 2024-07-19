using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.CldApi;
using Vanara.Extensions;
using Vanara.PInvoke;

namespace CloudSyncDriveClient.Services
{
    internal class CloudFileSyncHandlers
    {

        private static object _lock = new object();

        public static void OnFetchPlaceHolders(in CF_CALLBACK_INFO callbackInfo, in CF_CALLBACK_PARAMETERS param)
        {
            lock (_lock)
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

                IntPtr rawPlaceholders = default;
                int count = 0;

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

                if (hresult != HRESULT.S_OK)
                {
                    Console.Write($"Failed with error {hresult}");
                    hresult.ThrowIfFailed($"Failed with error {hresult}");
                }

                Console.WriteLine($"Fetched placeholders for '{filename}'");
            }
        }

        public static void OnFetchData(in CF_CALLBACK_INFO callbackInfo, in CF_CALLBACK_PARAMETERS param)
        {
            Console.WriteLine("OnFetchData");
        }

        public static void OnCancelFetchData(in CF_CALLBACK_INFO callbackInfo, in CF_CALLBACK_PARAMETERS param)
        {
            Console.WriteLine("OnCancelFetchData");
        }
    }
}
