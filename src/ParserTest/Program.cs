using DodjiParser;
using DodjiParser.Models;
using InfoParser;
using InfoParser.Models.JSON;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using InfoParser.LocalDatabase;
using InfoParser.Models;
using Microsoft.EntityFrameworkCore;
using SharedModel;

namespace ParserTest
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                RunTests();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        private static async Task RunTests()
        {
            await LocalDbTest();

            //await ParserTest();

            //FolderObserverTest();

            //await TestObserver();

            //await TestParser();
        }


        private static async Task TestDataAccess()
        {
            var init = await DodjiService.GetInstance(true);

            await init.Repository.AddCollection("my collection");
            var collection = (await init.Repository.GetCollections()).First();
            var cols2 = await init.Repository.GetCollections();
            var gals = await init.Repository.GetGalleries();

            await init.Repository.RemoveCollection(collection.Id);
            cols2 = await init.Repository.GetCollections();
            await init.Repository.AddCollection("my collection");
            cols2 = await init.Repository.GetCollections();
            await init.Repository.RenameCollection(cols2.First().Id, "my new collection");
            cols2 = await init.Repository.GetCollections();



            //await init.Repository.AddDestinationFolder(
            //    new DestinationFolder
            //    {
            //        CollectionId = collection.Id,
            //        Path = "Y:\\!playgoround\\!dest_mixed3"
            //    });
            //await init.Repository.AddSourceFolder(
            //    new SourceFolder
            //    {
            //        CollectionId = collection.Id,
            //        Path = "Y:\\!playgoround\\!source_files",
            //        KeepRelativePath = false
            //    });
            //await init.Repository.AddSourceFolder(
            //    new SourceFolder
            //    {
            //        CollectionId = collection.Id,
            //        Path = "Y:\\!playgoround\\!source_folders",
            //        KeepRelativePath = false
            //    });
        }

        private static async Task TestObserver()
        {
            var init = await DodjiService.GetInstance(true);
            await init.Repository.AddCollection("my collection");
            var collection = (await init.Repository.GetCollections()).First();
            await init.Repository.AddDestinationFolder(
                new DestinationFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!dest_mixed3"
                });
            await init.Repository.AddSourceFolder(
                new SourceFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!source_files",
                    KeepRelativePath = false
                });
            await init.Repository.AddSourceFolder(
                new SourceFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!source_folders",
                    KeepRelativePath = false
                });
        }

        private static async Task TestParser()
        {
            var init = await DodjiService.GetInstance(true);
            await init.Repository.AddCollection("my collection");
            var collection = (await init.Repository.GetCollections()).First();
            //await init.Repository.AddDestinationFolder(
            //    new DestinationFolder
            //    {
            //        CollectionId = collection.Id,
            //        Path = "Y:\\!playgoround\\!dest_mixed3"
            //    });
            await init.Repository.AddSourceFolder(
                new SourceFolder
                {
                    CollectionId = collection.Id,
                    Path = "Y:\\!playgoround\\!source_files",
                    KeepRelativePath = false
                });
        }

        private static async Task FolderObserverTest()
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
            //var parser = new EHentaiParser(EhentaiType.Ehentai);
            var parser = new ChaikaParser();
            //var gal = await parser.GetGallery(992385, "669d450607");
            //var tags = gal.Tags.ToList();
            var gal = await parser.GetGallery(2930);
            var tags = gal.Tags.ToList();

            var searchResult = await parser.SearchGalleries(GalleryCategory.Doujinshi | GalleryCategory.Manga, "sad");

            var i = searchResult.ToList();
        }

        private static async Task LocalDbTest()
        {
            using (var db = new LocalDbSourceContext())
            {
                var i = await db.ViewerGallery.Include(x => x.ViewerGalleryTags).ThenInclude(x => x.Tag).ToListAsync();
                Console.WriteLine(i.Count());
            }
        }
    }
}
