using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using DodjiParser.Models;
using InfoParser;
using InfoParser.Models;
using NLog;
using Source = DataAccess.Models.Source;

namespace DodjiParser
{
    internal class ParsersRepository
    {
        #region Static members

        private static Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("Parsers initialization");

            try
            {
                var localDbParser = new LocalDbParser();
                _parserList.Add(localDbParser);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot create localdb parser. Make sure that the file 'exhentai.db' exists in the root of program folder.");
            }

            try
            {
                var chaikaParser = new ChaikaParser();
                _parserList.Add(chaikaParser);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot create chaika parser.");
            }

            try
            {
                var eHentaiParser = Configuration.Instance.ExhentaiConfiguration != null 
                    ? new EHentaiParser(EhentaiType.Exhentai, Configuration.Instance.ExhentaiConfiguration) 
                    : new EHentaiParser();
                _parserList.Add(eHentaiParser);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot create e-hentai parser.");
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
}