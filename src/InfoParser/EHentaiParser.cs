using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NLog;

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

        private enum RequestType
        {
            Get,
            Post
        }

        #endregion Types

        #region Consts


        private const string BASE_EHENTAI_URL = "https://g.e-hentai.org/";
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

        private async Task<HtmlDocument> EHentaiRequest(string path, RequestType type = RequestType.Get, object jsonBody = null)
        {
            await _semaphore.WaitAsync();

            var interval = _lastAccess - DateTime.Now;

            if (interval < new TimeSpan(0, 0, 0, 1))
            {
                await Task.Delay(interval.Milliseconds);
            }
            _lastAccess = DateTime.Now;

            try
            {
                Logger.Trace($"{type} request — url: {path}; body: {jsonBody ?? "Empty"}");
                var document = new HtmlDocument();

                if (type == RequestType.Get)
                {
                    var result = await _client.GetStreamAsync(path);

                    document.Load(result);
                }
                // type == RequestType.Post
                else
                {
                    var stringContent = new StringContent(jsonBody == null
                        ? String.Empty
                        : jsonBody.ToString());

                    var result = await _client.PostAsync(path, stringContent);
                    result.EnsureSuccessStatusCode();

                    var responseStream = await result.Content.ReadAsStreamAsync();

                    document.Load(responseStream);

                }

                Logger.Trace($"{type} request — url: {path}\nResponse: {document.DocumentNode.OuterHtml.Substring(0, 50)}");
                return document;
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

        #endregion Private methods

        #region Public methods

        public async Task<EGalleryM> GetGallery(int id, string key)
        {
            await EHentaiRequest(API_URL, RequestType.Post, )
        }

        #endregion Public methods
    }
}
