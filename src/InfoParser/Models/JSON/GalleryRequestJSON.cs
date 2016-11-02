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
    public class GalleryRequestJson : JsonBase
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
        /// <param name="galleries">
        /// int - id; 
        /// string - token.
        /// 25 elements is maximum.
        /// </param>
        public GalleryRequestJson(List<Tuple<int, string>> galleries)
        {
            if (!galleries.Any() || galleries.Count > 25)
            {
                throw new AggregateException($"List '{nameof(galleries)}' should contain [1;25] elements");
            }

            FillGalleries(galleries);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public GalleryRequestJson(int id, string token)
        {
            var list = new List<Tuple<int, string>> { new Tuple<int, string>(id, token) };
            FillGalleries(list);
        }

        private void FillGalleries(List<Tuple<int, string>> galleries)
        {
            IList<IList<object>> gals = galleries.Select(x =>
            {
                IList<object> list = new List<object>();
                list.Add(x.Item1);
                list.Add(x.Item2);
                return list;
            }).ToList();

            GalleryList = gals;
        }
}
}
