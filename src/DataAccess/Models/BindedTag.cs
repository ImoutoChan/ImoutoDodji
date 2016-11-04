using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class BindedTag
    {
        public string Value { get; set; }



        [Required]
        public int GalleryId { get; set; }

        public Gallery Gallery { get; set; }

        [Required]
        public int TagId { get; set; }

        public Tag Tag { get; set; }

        [Required]
        public int NamespaceId { get; set; }

        public Namespace Namespace { get; set; }
    }
}