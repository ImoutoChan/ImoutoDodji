using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class DestinationFolder : EntityBase
    {
        [Required]
        public string Path { get; set; }



        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }
}