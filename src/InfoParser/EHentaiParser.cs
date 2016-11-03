using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfoParser.Models.JSON;
using Newtonsoft.Json;
using NLog;
using System.Net;
using HtmlAgilityPack;

namespace InfoParser
{
    public class EHentaiParser
    {
        #region Singleton

        private static readonly EHentaiParser _instance = new EHentaiParser();

        static EHentaiParser() { }

        public static EHentaiParser Instance => _instance;

        #endregion Singleton

        #region Types

        public class GalleryInfo
        {
            public int Id { get; set; }

            public string Token { get; set; }

            public string Url { get; set; }

            public string FullName { get; set; }

            public string PreviewUrl { get; set; }
        }

        private enum RequestType
        {
            Get,
            Post
        }

        #endregion Types

        #region Consts


        private const string BASE_EHENTAI_URL = "http://g.e-hentai.org/";
        private const string API_URL = "/api.php";

        #endregion Consts

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private DateTime _lastAccess = DateTime.Now;
        private readonly HttpClient _client = new HttpClient();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        #endregion Fields

        #region Constructors

        private EHentaiParser()
        {
            _client.BaseAddress = new Uri(BASE_EHENTAI_URL);
        }

        #endregion Constructors

        #region Private methods

        private async Task<string> EHentaiRequest(string path, RequestType type = RequestType.Get, 
                                                  Dictionary<string, string>  urlParameters = null, object jsonBody = null)
        {
            await _semaphore.WaitAsync();

            var interval = DateTime.Now - _lastAccess;

            if (interval < new TimeSpan(0, 0, 0, 1))
            {
                await Task.Delay(interval.Milliseconds);
            }
            _lastAccess = DateTime.Now;

            try
            {
                Logger.Trace($"{type} request — url: {path}; body: {jsonBody ?? "Empty"}");

                string responseString;

                if (type == RequestType.Get)
                {
                    var result = await _client.GetStringAsync(path + ToQueryString(urlParameters));

                    responseString = result;
                }
                // type == RequestType.Post
                else
                {
                    var stringContent = new StringContent(jsonBody == null
                        ? String.Empty
                        : jsonBody.ToString());

                    var result = await _client.PostAsync(path + ToQueryString(urlParameters), stringContent);
                    result.EnsureSuccessStatusCode();

                    var responseStream = await result.Content.ReadAsStreamAsync();

                    responseString = (new StreamReader(responseStream)).ReadToEnd();
                }

                Logger.Trace($"{type} request — url: {path}\nResponse: {responseString.Substring(0, 50)}");
                return responseString;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string ToQueryString(Dictionary<string, string> parameters)
        {
            if (parameters.Count == 0)
            {
                return String.Empty;
            }

            var array = parameters
                .Select(p => string.Format("{0}={1}", WebUtility.UrlEncode(p.Key), WebUtility.UrlEncode(p.Value)))
                .ToArray();
            return "?" + string.Join("&", array);
        }

        private IEnumerable<GalleryInfo> ParseGalleries(string result)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            var galNodes = doc.DocumentNode.SelectNodes("//table[@class=\"itg\"]/tr[@class]");

            if (galNodes == null)
            {
                yield break;
            }

            foreach (var trNode in galNodes)
            {
                yield return ParseGallery(trNode);
            }
        }

        private GalleryInfo ParseGallery(HtmlNode trNode)
        {
            var galleryInfo = new GalleryInfo();

            var text = trNode.InnerHtml;
            var previewNode = trNode.SelectSingleNode("td/div/div[@class=\"it2\"]/img");

            if (previewNode != null)
            {
                galleryInfo.PreviewUrl = previewNode.Attributes["src"].Value;
                galleryInfo.FullName = previewNode.Attributes["alt"].Value;
            }
            else
            {
                previewNode = trNode.SelectSingleNode("td/div/div[@class=\"it2\"]");
                var initString = previewNode.InnerText;

                var initStringParts = initString.Split('~');
                galleryInfo.PreviewUrl = $"http{(initStringParts[0] == "inits" ? "s" : String.Empty)}://" +
                                         $"{initStringParts[1]}/{initStringParts[2]}";
                galleryInfo.FullName = initStringParts[3];
            }


            var url = trNode.SelectSingleNode("//div[@class=\"it5\"]/a").Attributes["href"].Value;
            galleryInfo.Url = url;

            var parts = url.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            galleryInfo.Id = Int32.Parse(parts[parts.Length - 2]);
            galleryInfo.Token = parts[parts.Length - 1];

            return galleryInfo;
        }
        #endregion Private methods

        #region Public methods

        public async Task<Gallery> GetGallery(int id, string token)
        {
            var result = await EHentaiRequest(API_URL, RequestType.Post, jsonBody: new GalleryRequestJson(id, token));

            return JsonConvert.DeserializeObject<GalleryResponseJson>(result).GalleryList.FirstOrDefault();
        }

        public async Task<IEnumerable<GalleryInfo>> SearchGalleries(GalleryCategory categoriesForSearch = GalleryCategory.All, 
                                                                string searchString = "")
        {
            var dic = new Dictionary<string, string>();

            dic.Add("f_doujinshi", (categoriesForSearch.HasFlag(GalleryCategory.Doujinshi) ? 1 : 0).ToString());
            dic.Add("f_manga", (categoriesForSearch.HasFlag(GalleryCategory.Manga) ? 1 : 0).ToString());
            dic.Add("f_artistcg", (categoriesForSearch.HasFlag(GalleryCategory.ArtistCgSets) ? 1 : 0).ToString());
            dic.Add("f_gamecg", (categoriesForSearch.HasFlag(GalleryCategory.GameCgSets) ? 1 : 0).ToString());
            dic.Add("f_western", (categoriesForSearch.HasFlag(GalleryCategory.Western) ? 1 : 0).ToString());
            dic.Add("f_non-h", (categoriesForSearch.HasFlag(GalleryCategory.NonH) ? 1 : 0).ToString());
            dic.Add("f_imageset", (categoriesForSearch.HasFlag(GalleryCategory.ImageSets) ? 1 : 0).ToString());
            dic.Add("f_cosplay", (categoriesForSearch.HasFlag(GalleryCategory.Cosplay) ? 1 : 0).ToString());
            dic.Add("f_asianporn", (categoriesForSearch.HasFlag(GalleryCategory.AsianPorn) ? 1 : 0).ToString());
            dic.Add("f_misc", (categoriesForSearch.HasFlag(GalleryCategory.Misc) ? 1 : 0).ToString());

            dic.Add("f_search", searchString);
            dic.Add("f_apply", "Apply+Filter");

            var result = await EHentaiRequest(String.Empty, RequestType.Get, dic);
            
            IEnumerable<GalleryInfo> gals = ParseGalleries(result);

            return gals;
        }

        #endregion Public methods
    }
}
