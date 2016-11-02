using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace InfoParser.Models.JSON
{
    /// <summary>
    /// Url: http://g.e-hentai.org/api.php / POST
    /// Sample: 
    ///  {
    ///    "method": "gdata",
    ///    "gidlist": [
    ///      [
    ///        639967, "e2be237948"
    ///      ]
    ///    ],
    ///    "namespace": 1
    ///  }
    /// </summary>
    public class GalleryRequestJSON : JSONBase
    {
        /// <summary>
        /// "gdata"
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; set; } = "gdata";

        /// <summary>
        /// Gallery list for metadata
        /// First list of galleries, second contains id and key
        /// </summary>
        [JsonProperty("gidlist")]
        public IList<IList<object>> GalleryList { get; set; }

        /// <summary>
        /// 1 - include namespace to all tags
        /// 0 - does not include
        /// </summary>
        [JsonProperty("namespace")]
        public int Namespace { get; set; } = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="galleries">int - gallery.id; string - gallery.key</param>
        public GalleryRequestJSON(List<Tuple<int, string>> galleries)
        {
            if (!galleries.Any())
            {
                throw new AggregateException(nameof(galleries));
            }

            GalleryList = galleries.Select(x =>
            {
                IList list = new List<object>();
                list.Add(x.Item1);
                list.Add(x.Item2);
                return list;
            }).ToList() as IList<IList<object>>;
        }
    }
}
