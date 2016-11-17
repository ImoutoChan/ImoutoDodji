using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using InfoParser;
using InfoParser.Models;
using NLog.LayoutRenderers;

namespace DodjiParser
{
    internal enum ParserType
    {
        Ehentai,
        Exhentai,
        Chaika,
        LocalDatabase
    }

    internal class ParserEngine
    {
        #region Static members

        public static async Task<ParserEngine> GetInstance(DataRepository repository)
        {
            var pe = new ParserEngine(repository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        #region Fields

        private readonly DataRepository _repository;
        private List<IParser> _parserList = new List<IParser>();
        private readonly List<ParsingState> _enqueuedParsingStates = new List<ParsingState>();
        private readonly Queue<ParsingState> _queue = new Queue<ParsingState>();
        private object _queueLocker = new object();

        #endregion

        #region Constructors

        private ParserEngine(DataRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Private methods

        private async Task Initialize()
        {
            try
            {
                var localDbParser = new LocalDbParser();
                _parserList.Add(localDbParser);
            }
            catch (Exception)
            {
                // TODO log
            }

            try
            {
                var chaikaParser = new ChaikaParser();
                _parserList.Add(chaikaParser);
            }
            catch (Exception)
            {
                // TODO log
            }

            try
            {
                // TODO select if login/pass from exhentai
                var eHentaiParser = new EHentaiParser();
                _parserList.Add(eHentaiParser);
            }
            catch (Exception)
            {
                // TODO log
            }

            _repository.GalleriesChanged += RepositoryOnGalleriesChanged;

            await LoadNewGalleries();
        }

        private async void RepositoryOnGalleriesChanged(object sender, EventArgs eventArgs)
        {
            await LoadNewGalleries();
        }

        private async Task LoadNewGalleries()
        {
            var parsingStates = await _repository.GetParsingStates(x => x.State == GalleryState.Init);

            lock (_queue)
                lock (_enqueuedParsingStates)
                {
                    var newParsingStates = parsingStates
                        .Where(ps => _enqueuedParsingStates.All(eps => eps.GalleryId != ps.Gallery.Id))
                        .ToList();

                    _enqueuedParsingStates.AddRange(newParsingStates);

                    foreach (var newParsingState in newParsingStates)
                    {
                        _queue.Enqueue(newParsingState);
                    }
                }

            await RestartQueueProcess();
        }

        private async Task RestartQueueProcess()
        {
            if (Monitor.TryEnter(_queueLocker))
            {
                try
                {
                    bool whileFlag;
                    lock (_queue)
                    {
                        whileFlag = _queue.Any();
                    }

                    while (whileFlag)
                    {
                        ParsingState currentParsingState;
                        lock (_queue)
                        {
                            currentParsingState = _queue.Dequeue();
                        }

                        try
                        {
                            await ProcessParsingState(currentParsingState);
                        }
                        catch (Exception ex)
                        {
                            await
                                _repository.SetParsingStatus(currentParsingState.Id,
                                    GalleryState.SearchError,
                                    ex.Message);
                            // TODO log
                        }
                        finally
                        {
                            lock (_enqueuedParsingStates)
                            {
                                _enqueuedParsingStates.Remove(
                                    _enqueuedParsingStates.First(x => x.Id == currentParsingState.Id));
                            }
                        }

                        lock (_queue)
                        {
                            whileFlag = _queue.Any();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_queueLocker);
                }
            }
        }

        private async Task ProcessParsingState(ParsingState currentParsingState)
        {
            var searchFor = currentParsingState.Gallery.Name;

            for (var parserId = 0; parserId < _parserList.Count; parserId++)
            {
                var currentParset = _parserList[parserId];
                var isLastParser = parserId == _parserList.Count - 1;

                var searchResult = (await currentParset.SearchGalleries(searchString: searchFor)).ToList();

                if (!searchResult.Any())
                {
                    if (isLastParser)
                    {
                        await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchNotFound);
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (searchResult.Count > 1)
                {
                    // TODO selection
                    Task.WaitAll(
                        _repository.AddSearchResults(searchResult.Select(x => GetGalleryInfo(x, currentParsingState))),
                        _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchFoundAndWaitingForSelect));
                    break;
                }
                else // if (searchResult.Count() == 1)
                {
                    Task.WaitAll(
                        _repository.AddSearchResults(searchResult.Select(x => GetGalleryInfo(x, currentParsingState, true))), 
                        _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchFoundAndSelected)
                        );
                    var searchResultEntry = searchResult.Single();

                    // not awaited
                    ParseGallery(_parserList[parserId], currentParsingState, searchResultEntry.Id, searchResultEntry.Token);
                    break;
                }
            }
        }

        private SearchResult GetGalleryInfo(GalleryInfo x, ParsingState currentParsingState, bool isSelected = false)
        {
            return new SearchResult
            {
                GalleryId = x.Id,
                Token = x.Token,
                FullName = x.FullName,
                Url = x.Url,
                PreviewUrl = 
                x.PreviewUrl,
                ParsingStateId = currentParsingState.Id,
                Source = (DataAccess.Models.Source)(int)x.Source,
                IsSelected = isSelected
            };
        }

        private async Task ParseGallery(IParser parser, ParsingState currentParsingState, int id, string token)
        {
            try
            {
                // TODO parse gallery
                var galleryResult = await parser.GetGallery(id, token);



                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingParsed);
            }
            catch (Exception ex)
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingError, ex.Message);
            }
        }

        #endregion
    }
}