using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedModel;

namespace InfoParser.Models.JSON
{
    public class ChaikaGallery : JsonBase, IGallery
    {
        [JsonProperty("gallery")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_jpn")]
        public string TitleJpn { get; set; }

        [JsonProperty("category")]
        public GalleryCategory Category { get; set; }
        
        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("posted")]
        public long PostedDate { get; set; }

        [JsonProperty("filecount")]
        public int FileCount { get; set; }

        [JsonProperty("filesize")]
        public long FileSize { get; set; }

        [JsonProperty("expunged")]
        public bool IsExpunged { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("fjord")]
        public bool Fjord { get; set; }

        [JsonProperty("tags")]
        public IList<string> TagStrings { get; set; }

        public IEnumerable<Tag> Tags
        {
            get
            {
                return TagStrings.Select(ts =>
                {
                    var strings = ts.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);

                    if (strings.Length == 1)
                    {
                        return new Tag { Namespace = "misc", Name = strings[0] };
                    }

                    return new Tag {Namespace = strings[0], Name = strings[1]};
                });
            }
        }
    }
}