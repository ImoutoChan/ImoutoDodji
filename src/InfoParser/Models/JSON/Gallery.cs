using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedModel;

namespace InfoParser.Models.JSON
{
    public class ExGallery : IGallery
    {
        [JsonProperty("gid")]
        public int Id { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("archiver_key")]
        public string ArchiverKey { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_jpn")]
        public string TitleJpn { get; set; }

        [JsonProperty("category")]
        public GalleryCategory Category { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("posted")]
        public long PostedDate { get; set; }

        public DateTime PostedDateTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(PostedDate).DateTime;
            }
            set
            {
                PostedDate = (long) (value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            }
        }

        [JsonProperty("filecount")]
        public int FileCount { get; set; }

        [JsonProperty("filesize")]
        public long FileSize { get; set; }

        [JsonProperty("expunged")]
        public bool IsExpunged { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("torrentcount")]
        public int Torrentcount { get; set; }

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