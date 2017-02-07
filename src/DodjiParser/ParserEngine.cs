using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using InfoParser;
using InfoParser.Models;
using InfoParser.Models.JSON;
using NLog.LayoutRenderers;
using Source = DataAccess.Models.Source;

namespace DodjiParser
{
    internal enum ParserType
    {
        Ehentai,
        Exhentai,
        Chaika,
        LocalDatabase
    }

    internal class ParsersRepository
    {
        #region Static members
        
        public static async Task<ParsersRepository> GetInstance(DataRepository repository)
        {
            var pe = new ParsersRepository(repository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        private readonly DataRepository _repository;
        private readonly List<IParser> _parserList = new List<IParser>();

        private ParsersRepository(DataRepository repository)
        {
            _repository = repository;
        }

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
        }

        public List<IParser> Parsers => _parserList;

        public IParser this[Source searchResultSource]
        {
            get
            {
                switch (searchResultSource)
                {
                    case Source.Chaika:
                        return Parsers.OfType<ChaikaParser>().Single();
                    case Source.LocalDb:
                        return Parsers.OfType<LocalDbParser>().Single();
                    case Source.Ehentai:
                        return Parsers.OfType<EHentaiParser>().First(x => x.Type == EhentaiType.Ehentai);
                    case Source.Exhentai:
                        return Parsers.OfType<EHentaiParser>().First(x => x.Type == EhentaiType.Exhentai);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(searchResultSource), searchResultSource, null);
                }
            }
        }
    }

    internal class SearchEngine
    {
        #region Static members

        public static async Task<SearchEngine> GetInstance(DataRepository repository, ParsersRepository parsersRepository)
        {
            var pe = new SearchEngine(repository, parsersRepository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        #region Fields

        private readonly DataRepository _repository;
        private readonly ParsersRepository _parsersRepository;
        private readonly List<ParsingState> _enqueuedParsingStates = new List<ParsingState>();
        private readonly Queue<ParsingState> _queue = new Queue<ParsingState>();
        private readonly object _queueLocker = new object();

        #endregion

        #region Constructors

        private SearchEngine(DataRepository repository, ParsersRepository parsersRepository)
        {
            _repository = repository;
            _parsersRepository = parsersRepository;
        }

        #endregion

        #region Private methods

        private async Task Initialize()
        {
            _repository.GalleriesChanged += RepositoryOnGalleriesChanged;

            await LoadNewGalleries();
        }

        private async void RepositoryOnGalleriesChanged(object sender, EventArgs eventArgs)
        {
            await LoadNewGalleries();
        }

        private async Task LoadNewGalleries()
        {
            // todo errors?
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
            if (!Monitor.TryEnter(_queueLocker))
            {
                return;
            }

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

        private async Task ProcessParsingState(ParsingState currentParsingState)
        {
            var searchFor = currentParsingState.Gallery.Name;

            for (var parserId = 0; parserId < _parsersRepository.Parsers.Count; parserId++)
            {
                var currentParset = _parsersRepository.Parsers[parserId];
                var isLastParser = parserId == _parsersRepository.Parsers.Count - 1;

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
                    
                    OnNewGallerySelected();
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
                PreviewUrl = x.PreviewUrl,
                ParsingStateId = currentParsingState.Id,
                Source = (DataAccess.Models.Source)(int)x.Source,
                IsSelected = isSelected
            };
        }

        public async void OnNewGalleriesAppeared()
        {
            await LoadNewGalleries();
        }
        #endregion


        public event EventHandler NewGalleriesSelected;

        protected virtual void OnNewGallerySelected()
        {
            NewGalleriesSelected?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ParserEngine
    {
        #region Static members

        public static async Task<ParserEngine> GetInstance(DataRepository repository, ParsersRepository parsersRepository)
        {
            var pe = new ParserEngine(repository, parsersRepository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        private readonly DataRepository _repository;
        private readonly ParsersRepository _parsersRepository;
        private readonly List<ParsingState> _enqueuedParsingStates = new List<ParsingState>();
        private readonly Queue<ParsingState> _queue = new Queue<ParsingState>();
        private readonly object _queueLocker = new object();

        #region Constructors

        private ParserEngine(DataRepository repository, ParsersRepository parsersRepository)
        {
            _repository = repository;
            _parsersRepository = parsersRepository;
        }

        #endregion

        private async Task Initialize()
        {
            await LoadNewSearchResults();
        }

        private async Task LoadNewSearchResults()
        {
            var parsingStates = await _repository.GetParsingStates(x => x.State == GalleryState.SearchFoundAndSelected);

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
            if (!Monitor.TryEnter(_queueLocker))
            {
                return;
            }

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
                                GalleryState.ParsingError,
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

        private async Task ProcessParsingState(ParsingState currentParsingState)
        {
            var searchResult = await _repository.GetSingleSearchResult(currentParsingState.Id);
            var currentParser = _parsersRepository[searchResult.Source];

            await ParseGallery(currentParser, currentParsingState, searchResult.Id, searchResult.Token);
        }

        private async Task ParseGallery(IParser parser, ParsingState currentParsingState, int id, string token)
        {
            try
            {
                // TODO Save results to tag/metadata
                var galleryResult = await parser.GetGallery(id, token);
                
                await _repository.SetParsedGallery(GetParsedGallery(galleryResult, parser, currentParsingState));

                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingParsed);

                OnNewGalleriesParsed();
            }
            catch (Exception ex)
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingError, ex.Message);
            }
        }

        private ParsedGallery GetParsedGallery(IGallery galleryResult, IParser parser, ParsingState currentParsingState)
        {
            var parsedGallery = new ParsedGallery
            {
                ParsingStateId = currentParsingState.Id
            };


            var @switch = new Dictionary<Type, Action>
            {
                {
                    typeof(ChaikaParser), () => parsedGallery.Source = Source.Chaika
                },
                {
                    typeof(LocalDbParser), () => parsedGallery.Source = Source.LocalDb
                },
                {
                    typeof(EHentaiParser), () =>
                    {
                        var ehp = parser as EHentaiParser;
                        parsedGallery.Source = ehp.Type == EhentaiType.Ehentai
                            ? Source.Ehentai
                            : Source.Exhentai;
                    }
                },
            };
            @switch[parser.GetType()]();

            // Common properties

            parsedGallery.GalleryId = galleryResult.Id;
            parsedGallery.Category = galleryResult.Category;
            parsedGallery.FileCount = galleryResult.FileCount;
            parsedGallery.FileSize = galleryResult.FileSize;
            parsedGallery.IsExpunged = galleryResult.IsExpunged;
            parsedGallery.PostedDate = galleryResult.PostedDateTime;
            parsedGallery.Rating = galleryResult.Rating;
            parsedGallery.Tags = galleryResult.Tags.Select(x => $"{x.Namespace}:{x.Name}");
            parsedGallery.Title = galleryResult.Title;
            parsedGallery.TitleJpn = galleryResult.TitleJpn;
            parsedGallery.Uploader = galleryResult.Uploader;
            parsedGallery.Url = galleryResult.Url;
            parsedGallery.DateAdded = DateTime.Now;

            // Chaika extra

            if (galleryResult is ChaikaGallery)
            {
                var chaikaGallery = galleryResult as ChaikaGallery;

                parsedGallery.Fjord = chaikaGallery.Fjord;
            }

            // Ehentai extra

            if (galleryResult is ExGallery)
            {
                var exGallery = galleryResult as ExGallery;

                parsedGallery.Torrentcount = exGallery.Torrentcount;
                parsedGallery.Thumb = exGallery.Thumb;
                parsedGallery.Token = exGallery.Token;
                parsedGallery.ArchiverKey = exGallery.ArchiverKey;
            }

            return parsedGallery;
        }

        public async void OnNewGalleriesSelected()
        {
            await LoadNewSearchResults();
        }


        public event EventHandler NewGalleriesParsed;

        protected virtual void OnNewGalleriesParsed()
        {
            NewGalleriesParsed?.Invoke(this, EventArgs.Empty);
        }
    }
}