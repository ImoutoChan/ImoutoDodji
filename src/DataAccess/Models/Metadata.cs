using SharedModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataAccess.Models
{

    public class Metadata : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Column("AlternativeNames")]
        public string AlternativeNamesCore { get; set; }

        public string Translator { get; set; }

        /// <summary>
        /// C87, C72, PF25, etc
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Add/Remove not supported
        /// </summary>
        [NotMapped]
        public List<string> AlternativeNames
        {
            get
            {
                return AlternativeNamesCore.Split(';').ToList();
            }
            set
            {
                AlternativeNamesCore = string.Join(";", value);
            }
        }

        [Required]
        public int GalleryId { get; set; }

        public Gallery Gallery { get; set; }
    }

    public class PandaMetadata : Metadata
    {
        [Required]
        public string Url { get; set; }

        [Required]
        public string Uploader { get; set; }

        [Required]
        public int PandaId { get; set; }

        [Required]
        public string PandaToken { get; set; }

        [Required]
        public string ArchiveKey { get; set; }

        [Required]
        public GalleryCategory Category { get; set; }

        [Required]
        public string ThumbUrl { get; set; }

        [Required]
        public DateTime PostedDate { get; set; }

        [Required]
        public int FileCount { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public bool IsExpunged { get; set; }

        [Required]
        public double Rating { get; set; }

        [Required]
        public int TorrentCount { get; set; }
    }
}
