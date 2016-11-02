using System;
using InfoParser;
using InfoParser.Models.JSON;
using System.Threading.Tasks;
using System.Linq;

namespace ParserTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LoadGallery();

            Console.ReadKey();
        }

        private async static Task LoadGallery()
        {
            var parser = EHentaiParser.Instance;
            //var gal = await parser.GetGallery(992385, "669d450607");
            //var tags = gal.Tags.ToList();

            var searchResult = await parser.SearchGalleries(GalleryCategory.Doujinshi | GalleryCategory.Manga, "sad");


        }
    }
}
