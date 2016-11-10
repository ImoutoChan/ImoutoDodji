namespace InfoParser.LocalDatabase
{
    public class ViewerGalleryTags
    {
        public long Id { get; set; }
        public long GalleryId { get; set; }
        public long TagId { get; set; }

        public virtual ViewerGallery Gallery { get; set; }
        public virtual ViewerTag Tag { get; set; }
    }
}
