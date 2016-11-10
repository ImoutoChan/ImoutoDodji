using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfoParser.Models.JSON
{
    public class GalleryResponseJson : JsonBase
    {
        [JsonProperty("gmetadata")]
        public IList<ExGallery> GalleryList { get; set; }
    }
}
