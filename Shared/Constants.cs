namespace Shared
{
    public class Constants
    {
        public class Dummy
        {
            public readonly static string SyncSource = @"D:\CFAPI\Source";
            public readonly static string SyncRoot = @"D:\CFAPI\Target_1";

            public readonly static string StorageProviderId = "E15StorageProvider";
            public readonly static string UserName = "TestAccount1";
            public readonly static string AccountId = UserName;
            public readonly static string RecycleBinWebUIUrl = $"https://www.bing.com/search?q=别几把点了这东西还没做";
        }

        public class Brand
        {
            public readonly static string SyncRootDisplayName = "E15 Sync Drive";
            public readonly static string SyncRootDisplayIcon = "%SystemRoot%\\system32\\charmap.exe,0";
        }

        public readonly static string Version = "1.0.0";
    }
}
