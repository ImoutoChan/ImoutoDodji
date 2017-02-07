using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfoParser.LocalDatabase;
using InfoParser.Models;
using InfoParser.Models.JSON;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SharedModel;

namespace InfoParser
{
    public class LocalDbParser : IParser
    {
        #region Constructors

        public LocalDbParser()
        {
            using (var db = new LocalDbSourceContext())
            {
                if (!(db.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                {
                    throw new Exception(
                        "Local database was not found. Make sure to place it in program folder and name 'exhentai.db'.");
                }
            }
        }

        #endregion
        
        #region Public methods

        public async Task<IGallery> GetGallery(int id, string token = null)
        {
            using (var db = new LocalDbSourceContext())
            {
                var dbGallery = await db.ViewerGallery
                    .Include(vg => vg.ViewerGalleryTags)
                    .ThenInclude(vgt => vgt.Tag)
                    .FirstAsync(vg => vg.Gid == id.ToString());

                var gallery = new ExGallery();

                gallery.Category = dbGallery.Category.ToEnum<GalleryCategory>();
                gallery.FileCount = dbGallery.Filecount ?? 0;
                gallery.FileSize = dbGallery.Filesize ?? 0;
                gallery.Id = Convert.ToInt32(dbGallery.Gid);
                gallery.IsExpunged = dbGallery.Expunged != "False";
                gallery.PostedDateTime = Convert.ToDateTime(dbGallery.Posted);
                gallery.Rating = Convert.ToDouble(dbGallery.Rating);
                gallery.TagStrings =
                    dbGallery.ViewerGalleryTags.Select(vgt => vgt.Tag).Select(t => $"{t.Scope}:{t.Name}").ToList();
                gallery.Title = dbGallery.Title;
                gallery.TitleJpn = dbGallery.TitleJpn;
                gallery.Token = dbGallery.Token;
                gallery.Uploader = dbGallery.Uploader;

                return gallery;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoriesForSearch">NotSupported</param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GalleryInfo>> SearchGalleries(GalleryCategory categoriesForSearch = GalleryCategory.All, 
            string searchString = "")
        {
            List<ViewerGallery> searchResult;
            using (var db = new LocalDbSourceContext())
            {
                var request =
                    db.ViewerGallery.Where(
                        g => (g.Title ?? String.Empty).IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);

                searchResult = await request.Take(20).ToListAsync();
            }

            return searchResult.Select(viewerGallery => new GalleryInfo
            {
                FullName = viewerGallery.Title,
                Id = Convert.ToInt32(viewerGallery.Gid),
                Source = Source.LocalDb,
            }).ToList();
        }
        
        #endregion Public methods
    }
}