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
