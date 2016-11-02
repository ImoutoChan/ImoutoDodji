using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace InfoParser.Models.JSON
{
    public enum GalleryCategory
    {
        [EnumMember(Value = "Doujinshi")]
        Doujinshi,

        [EnumMember(Value = "Manga")]
        Manga,

        [EnumMember(Value = "Artist CG Sets")]
        ArtistCgSets,

        [EnumMember(Value = "Game CG Sets")]
        GameCgSets,

        [EnumMember(Value = "Western")]
        Western,

        [EnumMember(Value = "Image Sets")]
        ImageSets,

        [EnumMember(Value = "Non-H")]
        NonH,

        [EnumMember(Value = "Cosplay")]
        Cosplay,

        [EnumMember(Value = "Asian Porn")]
        AsianPorn,

        [EnumMember(Value = "Misc")]
        Misc,

        [EnumMember(Value = "Private")]
        Private
    }

    public class Gmetadata
    {

        [JsonProperty("gid")]
        public int gid { get; set; }

        [JsonProperty("token")]
        public string token { get; set; }

        [JsonProperty("archiver_key")]
        public string archiver_key { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("title_jpn")]
        public string title_jpn { get; set; }

        [JsonProperty("category")]
        public GalleryCategory Category { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("posted")]
        public long PostedDate { get; set; }

        [JsonProperty("filecount")]
        public int FileCount { get; set; }

        [JsonProperty("filesize")]
        public long Filesize { get; set; }

        [JsonProperty("expunged")]
        public bool IsExpunged { get; set; }

        [JsonProperty("rating")]
        public bool Rating { get; set; }

        [JsonProperty("torrentcount")]
        public string Torrentcount { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }
    }

    public class GalleryResponseJSON : JSONBase
    {

        [JsonProperty("gmetadata")]
        public IList<Gmetadata> GalleryList { get; set; }
    }
}
