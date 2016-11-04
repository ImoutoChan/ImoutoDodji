using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Collection : EntityBase
    {
        [Required]
        public string Name { get; set; }



        public List<SourceFolder> SourceFolders { get; set; } 

        public DestinationFolder DestinationFolder { get; set; }
    }
}