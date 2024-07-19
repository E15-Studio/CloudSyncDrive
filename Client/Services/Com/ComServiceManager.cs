using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.Ole32;
using Vanara.PInvoke;
using CloudSyncDriveClient.Services.Com.Providers;

namespace CloudSyncDriveClient.Services.Com
{
    internal class ComServiceManager
    {
        private static Thread? workerThread;
        private static bool stop = true;

        public static void LaunchComServices()
        {
            if (workerThread?.IsAlive == true)
                throw new InvalidOperationException("COM Services are already running.");

            workerThread = new Thread(() =>
            {
                // Initialize COM Objects.
                uint cookie;
                var thumbnailProvider = new Thumbnail();
                CoRegisterClassObject(
                    typeof(Thumbnail).GUID, thumbnailProvider,
                    CLSCTX.CLSCTX_LOCAL_SERVER,
                    REGCLS.REGCLS_MULTIPLEUSE, out cookie).ThrowIfFailed();

                var explorerCommandProvider = new ExplorerCommand();
                CoRegisterClassObject(
                    typeof(ExplorerCommand).GUID, explorerCommandProvider,
                    CLSCTX.CLSCTX_LOCAL_SERVER,
                    REGCLS.REGCLS_MULTIPLEUSE, out cookie).ThrowIfFailed();

                var uriSource = new UriSource();
                CoRegisterClassObject(
                    typeof(UriSource).GUID, uriSource,
                    CLSCTX.CLSCTX_LOCAL_SERVER,
                    REGCLS.REGCLS_MULTIPLEUSE, out cookie).ThrowIfFailed();

                var customStateProvider = new CustomState();
                CoRegisterClassObject(
                    typeof(CustomState).GUID, customStateProvider,
                    CLSCTX.CLSCTX_LOCAL_SERVER,
                    REGCLS.REGCLS_MULTIPLEUSE, out cookie).ThrowIfFailed();

                using var dummyEvent = CreateEvent(null, false, false);
                if (dummyEvent.IsInvalid)
                    Win32Error.ThrowLastError();

                while (!stop)
                    CoWaitForMultipleHandles(COWAIT_FLAGS.COWAIT_DISPATCH_CALLS, 200, 1, new[] { (nint)dummyEvent }, out _);

            });
            stop = false;
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();
        }

        public static void StopComServices()
        {
            stop = true;

            if (workerThread?.IsAlive == false)
                return;

            workerThread?.Join();
        }
    }
}
