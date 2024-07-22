using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shared;
using Windows.Devices.Geolocation;
namespace CloudSyncDriveClient.SyncEngine
{
    internal class Directory
    {
        private string path;

        private Directory()
        {
            path = "";
        }

        public static Directory GetRemote(string path)
        {
            Directory dir = new();
            dir.path = path;
            return dir;
        }

        public DirectoryInfo[] GetSubDirectories()
        {
            var relPath = Path.Join(Constants.Dummy.SyncSource, path);
            if (!System.IO.Directory.Exists(relPath))
                throw new Exception("Directory does not exist");

            return new DirectoryInfo(relPath).GetDirectories();
        }

        public FileInfo[] GetFiles()
        {
            var relPath = Path.Join(Constants.Dummy.SyncSource, path);
            if (!System.IO.Directory.Exists(relPath))
                throw new Exception("Directory does not exist");

            return new DirectoryInfo(relPath).GetFiles();
        }
    }
}
