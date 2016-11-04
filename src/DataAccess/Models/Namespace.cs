using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Namespace : EntityBase
    {
        [Required]
        public string Name { get; set; }


        public List<BindedTag> BindedTags { get; set; }
    }
}