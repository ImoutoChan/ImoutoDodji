using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using InfoParser.Models;
using Source = DataAccess.Models.Source;

namespace DodjiParser
{
    internal class SearchEngine : ParsingStateEngineBase
    {
        #region Static members

        public static async Task<SearchEngine> GetInstance(DataRepository repository, ParsersRepository parsersRepository)
        {
            var pe = new SearchEngine(repository, parsersRepository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        #region Constructors

        private SearchEngine(DataRepository repository, ParsersRepository parsersRepository) : base(repository, parsersRepository)
        {
        }

        #endregion

        #region Private methods

        protected override async Task Initialize()
        {
            _repository.GalleriesChanged += RepositoryOnGalleriesChanged;

            await base.Initialize();
        }

        public override async Task UpdateStates()
        {
            await LoadNewParsingStates(GalleryState.Init);
        }

        protected override async Task ProcessParsingState(ParsingState currentParsingState)
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

                    OnParsingStateUpdated();
                    break;
                }
            }
        }

        private async void RepositoryOnGalleriesChanged(object sender, EventArgs eventArgs)
        {
            await LoadNewParsingStates(GalleryState.Init);
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
                Source = (Source)(int)x.Source,
                IsSelected = isSelected
            };
        }

        #endregion
    }
}