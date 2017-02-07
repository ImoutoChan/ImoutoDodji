using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using InfoParser;
using InfoParser.Models;
using Source = DataAccess.Models.Source;

namespace DodjiParser
{
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
}