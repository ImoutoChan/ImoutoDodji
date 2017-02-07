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
using NLog;
using Source = DataAccess.Models.Source;

namespace DodjiParser
{
    internal class ParserEngine : ParsingStateEngineBase
    {
        protected override Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        #region Static members

        public static async Task<ParserEngine> GetInstance(DataRepository repository, ParsersRepository parsersRepository)
        {
            var pe = new ParserEngine(repository, parsersRepository);
            await pe.Initialize();
            return pe;
        }

        #endregion

        #region Constructors

        private ParserEngine(DataRepository repository, ParsersRepository parsersRepository) : base(repository, parsersRepository)
        {
        }

        #endregion

        #region Methods

        protected override async Task Initialize()
        {
            await base.Initialize();

            Logger.Trace("Initialized");
        }

        public override async Task UpdateStates()
        {
            await LoadNewParsingStates(GalleryState.SearchFoundAndSelected);
        }

        protected override async Task ProcessParsingState(ParsingState currentParsingState)
        {
            var searchResult = await _repository.GetSingleSearchResult(currentParsingState.Id);
            var currentParser = _parsersRepository[searchResult.Source];
            
            await ParseGallery(currentParser, currentParsingState, searchResult.GalleryId, searchResult.Token);
        }

        private async Task ParseGallery(IParser parser, ParsingState currentParsingState, int id, string token)
        {
            try
            {
                // TODO Save results to tag/metadata


                Logger.Info($"Parsing gallery info {currentParsingState.Gallery.Name} from {parser.GetType().Name}...");
                var galleryResult = await parser.GetGallery(id, token);
                
                await _repository.SetParsedGallery(GetParsedGallery(galleryResult, parser, currentParsingState));

                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingParsed);

                OnParsingStateUpdated();
            }
            catch (Exception ex)
            {
                await _repository.SetParsingStatus(currentParsingState.Id, GalleryState.ParsingError, ex.Message);
                Logger.Error(ex, $"Error in parsing gallery info {currentParsingState.Gallery.Name} from {parser.GetType().Name}...");
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

        #endregion
    }
}