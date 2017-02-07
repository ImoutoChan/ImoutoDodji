using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using InfoParser.Models;
using InfoParser.Models.JSON;
using Newtonsoft.Json;
using NLog;
using SharedModel;

namespace InfoParser
{
    public class ChaikaParser : IParser
    {
        #region Consts

        private const string API_URL = "/api";
        private const int REQUEST_DELAY = 0;
        private const int ERROR_REQUEST_DELAY = 10;

        #endregion Consts

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private DateTime _lastAccess = DateTime.Now;
        private readonly HttpClient _client;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _lastRequestError = false;
        private readonly Uri BASE_CHAIKA_URL = new Uri("https://panda.chaika.moe/");

        #endregion Fields

        #region Constructors

        public ChaikaParser()
        {
            var cookie = GetCookie();
            _client = new HttpClient(new HttpClientHandler { CookieContainer = cookie}) { BaseAddress = BASE_CHAIKA_URL };
        }

        #endregion Constructors

        #region Private methods

        private CookieContainer GetCookie()
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookies };
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = BASE_CHAIKA_URL;
                var response = client.GetAsync(String.Empty).Result;
                response.EnsureSuccessStatusCode();
            }
            return cookies;
        }

        private async Task<string> ChaikaRequest(string path, Dictionary<string, string>  urlParameters = null)
        {
            await _semaphore.WaitAsync();

            var interval = DateTime.Now - _lastAccess;

            if (interval < new TimeSpan(0, 0, 0, _lastRequestError ? ERROR_REQUEST_DELAY : REQUEST_DELAY))
            {
                await Task.Delay(interval.Milliseconds);
            }
            _lastAccess = DateTime.Now;

            try
            {
                Logger.Trace($"Get request — url: {path}");

                var responseString = await _client.GetStringAsync(path + ToQueryString(urlParameters));
                
                Logger.Trace($"Get request — url: {path}\nResponse: {responseString.Substring(0, 50)}");
                return responseString;
            }
            catch (HttpRequestException ex)
            {
                _lastRequestError = true;
                Logger.Error(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _lastRequestError = true;
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
            if (parameters == null || parameters.Count == 0)
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

            var galNodes = doc.DocumentNode.SelectNodes("//div[@class=\"gallery\"]");

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
            var galleryInfo = new GalleryInfo {Source = Source.Chaika};

            var aNode = trNode.SelectSingleNode("a");
            galleryInfo.Url = BASE_CHAIKA_URL + aNode.Attributes["href"].Value.Substring(1);
            galleryInfo.Id = Convert.ToInt32(aNode.Attributes["href"].Value.Split(new [] {"/"}, StringSplitOptions.RemoveEmptyEntries).Last());

            var imgNode = aNode.SelectSingleNode("img");
            galleryInfo.PreviewUrl = imgNode.Attributes["src"].Value;
            galleryInfo.FullName = imgNode.Attributes["title"].Value;
            
            return galleryInfo;
        }

        private async Task<IEnumerable<GalleryInfo>> SearchGalleries(string searchString = "")
        {
            var dic = new Dictionary<string, string>();

            dic.Add("title", searchString);
            dic.Add("view", "cover");

            var result = await ChaikaRequest("/search", dic);

            IEnumerable<GalleryInfo> gals = ParseGalleries(result);

            return gals;
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token">Reductant</param>
        /// <returns></returns>
        public async Task<IGallery> GetGallery(int id, string token = null)
        {
            var dic = new Dictionary<string, string>();

            dic.Add("archive", id.ToString());

            var result = await ChaikaRequest(API_URL, urlParameters: dic);

            var resultObj = JsonConvert.DeserializeObject<ChaikaGallery>(result);
            resultObj.Url = BASE_CHAIKA_URL + $"archive/{id}/";

            return resultObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoriesForSearch">Not supported</param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GalleryInfo>> SearchGalleries(GalleryCategory categoriesForSearch = GalleryCategory.All, string searchString = "")
        {
            return await SearchGalleries(searchString);
        }
        
        #endregion Public methods
    }
}