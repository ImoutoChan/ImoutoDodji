using System.Collections.Generic;
using System.Threading.Tasks;
using SharedModel;

namespace InfoParser.Models
{
    public interface IParser
    {
        Task<IGallery>GetGallery(int id, string token);

        Task<IEnumerable<GalleryInfo>> SearchGalleries(GalleryCategory categoriesForSearch = GalleryCategory.All, string searchString = "");
    }
}