using System.ComponentModel.DataAnnotations;
using SharedModel;

namespace DataAccess.Models
{
    public class SourceFolder : EntityBase
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public ObservationType ObservationType { get; set; } = ObservationType.All;

        [Required]
        public bool KeepRelativePath { get; set; } = false;

        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }
}