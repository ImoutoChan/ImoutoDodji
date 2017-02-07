using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModel;

namespace DataAccess.Models
{
    public class ParsedGallery : EntityBase
    {
        [NotMapped]
        private const string _tagStringSeparator = "|'|";

        [Required]
        public int GalleryId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string TitleJpn { get; set; }

        [Required]
        public GalleryCategory Category { get; set; }

        [Required]
        public string Uploader { get; set; }

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
        public Source Source { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// |'| - separator
        /// </summary>
        [Required]
        public string TagStrings { get; set; }

        [NotMapped]
        public IEnumerable<string> Tags
        {
            get
            {
                return TagStrings.Split(new[] {_tagStringSeparator}, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                TagStrings = String.Join(_tagStringSeparator, value);
            }
        }

        #region Navigation properties

        [Required]
        public int ParsingStateId { get; set; }

        public ParsingState ParsingState { get; set; }

        #endregion

        #region NotRequired

        public int Torrentcount { get; set; }

        public string Thumb { get; set; }

        public string Token { get; set; }

        public string ArchiverKey { get; set; }

        public bool Fjord { get; set; }

        public string Url { get; set; }

        #endregion
    }
}