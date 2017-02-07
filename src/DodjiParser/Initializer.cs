using System.Threading.Tasks;
using DataAccess;
using NLog;
using Remotion.Linq.Parsing;

namespace DodjiParser
{
    public class DodjiService
    {
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public static async Task<DodjiService> GetInstance(bool wipe = false)
        {
            var ds = new DodjiService();
            await ds.Initialize(wipe);
            return ds;
        }

        private DataRepository _repository;
        private FileSystemObserver _fileSystemObserver;
        private SearchEngine _searchEngine;
        private ParserEngine _parserEngine;
        private ParsersRepository _parsersRepository;

        private DodjiService()
        {
        }

        public DataRepository Repository => _repository;
        
        private async Task Initialize(bool wipe)
        {
            Logger.Info($"Starting initialization...");

            _repository = await DataRepository.GetInstance(wipe);
            _parsersRepository = await ParsersRepository.GetInstance(_repository);

            _fileSystemObserver = await FileSystemObserver.GetInstance(_repository);

            _searchEngine = await SearchEngine.GetInstance(_repository, _parsersRepository);

            _parserEngine = await ParserEngine.GetInstance(_repository, _parsersRepository);
            _searchEngine.ParsingStateUpdated += async (sender, args) => await _parserEngine.UpdateStates();

            Logger.Info($"Initialization is finished.");
        }
    }
}
