using DodjiParser;
using DodjiParser.Models;
using InfoParser;
using InfoParser.Models.JSON;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using SharedModel;

namespace ParserTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunTests();

            Console.ReadKey();
        }

        private async static Task RunTests()
        {
            //await ParserTest();

            //FolderObserverTest();

            await TestObserver();
        }

        private static async Task TestObserver()
        {
            var init = new Initializer(true);
            await init.Repository.AddCollection("my collection");
            var collection = (await init.Repository.GetCollections()).First();
            await init.Repository.AddDestinationFolder(
                new DestinationFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!dest_mixed2"
                });
            await init.Repository.AddSourceFolder(
                new SourceFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!source_files",
                    KeepRelativePath = true
                });
            await init.Repository.AddSourceFolder(
                new SourceFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!source_folders",
                    KeepRelativePath = true
                });
        }

        private async static Task FolderObserverTest()
        {
            var obs = new FolderObserver(new DirectoryInfo(@"Y:\!!DodjiSource\!source"), ObservationType.All);
            obs.CurrentStateUpdated += (obj, args) =>
            {
                int i = 0;
                foreach (var fsGallery in args.FileSystemGalleries)
                {
                    Console.WriteLine(++i + "\t" + fsGallery.Path);
                    Console.WriteLine();
                }
                Console.WriteLine();
            };
        }

        private static async Task ParserTest()
        {
            var parser = EHentaiParser.Instance;
            //var gal = await parser.GetGallery(992385, "669d450607");
            //var tags = gal.Tags.ToList();
            var gal = await parser.GetGallery(735011, "b020881a70");
            var tags = gal.Tags.ToList();

            //var searchResult = await parser.SearchGalleries(GalleryCategory.Doujinshi | GalleryCategory.Manga, "sad");

            //var i = searchResult.ToList();
        }
    }
}
