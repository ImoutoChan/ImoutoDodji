namespace InfoParser.Models
{
    public class GalleryInfo
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public string Url { get; set; }

        public string FullName { get; set; }

        public string PreviewUrl { get; set; }

        public Source Source { get; set; }
    }

    public enum Source
    {
        Exhentai,
        Ehentai,
        Chaika,
        LocalDb
    }
}