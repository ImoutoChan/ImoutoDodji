using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Gallery : EntityBase
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public int FilesCount { get; set; }

        [Required]
        public string PreviewPath { get; set; }

        [Required]
        public string Md5 { get; set; }

        [Required]
        public StorageType StorageType { get; set; }



        public List<Metadata> Metadata { get; set; }

        public List<ParsingState> ParsingStates { get; set; }

        public List<BindedTag> BindedTags { get; set; }

        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }
}