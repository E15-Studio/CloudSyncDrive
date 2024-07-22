
using System;
using System.Threading;

using static Shared.Constants;

using CloudSyncDriveClient.Services;
using Windows.Graphics.Capture;
using System.IO;

namespace CloudSyncDriveClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo a = new("a");

            Console.WriteLine("Cloud Sync Drive Client");
            Console.WriteLine($"Mouting {Dummy.SyncRoot} to {Dummy.SyncSource}");

            CloudFileProvider.Start(Dummy.SyncRoot);

            Console.WriteLine("Press Ctrl + C to stop gracefully.");

            bool stop = false;

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                stop = true;
            };

            while (!stop)
            {
                Thread.Sleep(1000);
            }

            CloudFileProvider.Stop();

        }
    }
}
