using System.Collections.Generic;

namespace InfoParser.LocalDatabase
{
    public class ViewerTag
    {
        public ViewerTag()
        {
            ViewerGalleryTags = new HashSet<ViewerGalleryTags>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        public string Source { get; set; }
        public string CreateDate { get; set; }

        public virtual ICollection<ViewerGalleryTags> ViewerGalleryTags { get; set; }
    }
}
