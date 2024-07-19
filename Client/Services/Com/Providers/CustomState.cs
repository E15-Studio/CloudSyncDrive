using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Provider;

namespace CloudSyncDriveClient.Services.Com.Providers
{
    [ComVisible(true), Guid("f0c9de6c-6c76-44d7-a58e-579cdf7af263")]
    public class CustomState : IStorageProviderItemPropertySource
    {
        public IEnumerable<StorageProviderItemProperty> GetItemProperties(string itemPath)
        {
            // This icon is just for the sample. You should provide your own branded icon here
            yield return new StorageProviderItemProperty { Id = 2, Value = "Value2", IconResource = "shell32.dll,-14" };
        }
    }
}
