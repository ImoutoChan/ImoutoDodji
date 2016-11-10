using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using InfoParser;
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
        private EHentaiParser _eHentaiParser;
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

        private async Task Initialize(ParserType type = ParserType.Exhentai)
        {
            // TODO
            //switch (type)
            //{
            //    case ParserType.Ehentai:
            //        _eHentaiParser = new EHentaiParser();
            //        _repository.GalleriesChanged += RepositoryOnGalleriesChanged;
            //        break;
            //    case ParserType.Exhentai:
            //        break;
            //    case ParserType.Chaika:
            //        break;
            //    case ParserType.LocalDatabase:
            //        break;
            //}

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

            var searchResult = (await _eHentaiParser.SearchGalleries(searchString: searchFor)).ToList();

            if (!searchResult.Any())
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchNotFound);
            }
            else if (searchResult.Count() > 1)
            {
                // TODO Fill search results
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchFoundAndWaitingForSelect);
            }
            else // if (searchResult.Count() == 1)
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.SearchFoundAndSelected);
                var searchResultEntry = searchResult.First();

                await ParseGallery(currentParsingState, searchResultEntry.Id, searchResultEntry.Token);
            }
        }

        private async Task ParseGallery(ParsingState currentParsingState, int id, string token)
        {
            try
            {
                // TODO parse gallery
                var galleryResult = await _eHentaiParser.GetGallery(id, token);
            }
            catch (Exception ex)
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingError, ex.Message);
            }
        }

        #endregion
    }
}