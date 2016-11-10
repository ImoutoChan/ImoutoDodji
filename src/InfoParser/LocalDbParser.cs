using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfoParser.Models;
using SharedModel;

namespace InfoParser
{
    public class LocalDbParser : IParser
    {
        #region Consts

        #endregion Consts

        #region Fields

        #endregion Fields

        #region Constructors

        public LocalDbParser()
        {
        }

        #endregion Constructors

       #region Public methods

        public async Task<IGallery> GetGallery(int id, string token)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<GalleryInfo>> SearchGalleries(GalleryCategory categoriesForSearch = GalleryCategory.All, 
            string searchString = "")
        {
            throw new NotImplementedException();
        }
        
        #endregion Public methods
    }
}