using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class ParsingState : EntityBase
    {
        [Required]
        public GalleryState State { get; set; }

        [Required]
        public DateTime DateTimeCreated { get; set; }

        [Required]
        public DateTime DateTimeUpdated { get; set; }


        [Required]
        public int GalleryId { get; set; }

        public Gallery Gallery { get; set; }
    }
}