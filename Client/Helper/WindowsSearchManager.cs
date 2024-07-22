using System;
using Vanara.InteropServices;
using static Vanara.PInvoke.SearchApi;

namespace CloudSyncDriveClient.Helper
{
    internal class WindowsSearchManager
    {
        /// <summary>
        /// If a folder is mounted by Cloud File API, it should be added to the search indexer
        /// Because if the folder is not indexed, the attampts to get the properties on items
        /// will not return the expected values.
        /// </summary>
        /// <param name="path"></param>
        public static void AddFolderToSearchIndexer(string path)
        {
            var url = "file:///" + path;

            using var searchManager = ComReleaserFactory.Create(new ISearchManager());
            using var searchCatalogManager = ComReleaserFactory.Create(searchManager.Item.GetCatalog("SystemIndex"));
            using var searchCrawlScopeManager = ComReleaserFactory.Create(searchCatalogManager.Item.GetCrawlScopeManager());
            searchCrawlScopeManager.Item.AddDefaultScopeRule(url, true, FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS);
            searchCrawlScopeManager.Item.SaveAll();

            Console.WriteLine($"Successfully added {path} to search indexer");

            return;
        }
    }
}
