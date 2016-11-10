using System.Collections.Generic;

namespace InfoParser.LocalDatabase
{
    public class ViewerGallery
    {
        public ViewerGallery()
        {
            ViewerGalleryTags = new HashSet<ViewerGalleryTags>();
        }

        public long Id { get; set; }
        public string Token { get; set; }
        public string Title { get; set; }
        public string TitleJpn { get; set; }
        public string Category { get; set; }
        public string Uploader { get; set; }
        public string Posted { get; set; }
        public int? Filecount { get; set; }
        public long? Filesize { get; set; }
        public string Expunged { get; set; }
        public string Rating { get; set; }
        public string Hidden { get; set; }
        public string Fjord { get; set; }
        public string DlType { get; set; }
        public string CreateDate { get; set; }
        public string LastModified { get; set; }
        public string Public { get; set; }
        public string Comment { get; set; }
        public string Gid { get; set; }

        public virtual ICollection<ViewerGalleryTags> ViewerGalleryTags { get; set; }
    }
}
