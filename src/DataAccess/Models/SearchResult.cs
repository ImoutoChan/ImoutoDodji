using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class SearchResult : EntityBase
    {
        [Required]
        public int GalleryId { get; set; }

        public string Token { get; set; }

        public string Url { get; set; }

        public string FullName { get; set; }

        public string PreviewUrl { get; set; }

        [Required]
        public Source Source { get; set; }

        [Required]
        public bool IsSelected { get; set; }


        [Required]
        public int ParsingStateId { get; set; }

        public ParsingState ParsingState { get; set; }
    }
}