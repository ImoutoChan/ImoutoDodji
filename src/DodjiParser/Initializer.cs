using System.Threading.Tasks;
using DataAccess;
using Remotion.Linq.Parsing;

namespace DodjiParser
{
    public class DodjiService
    {
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
            _repository = await DataRepository.GetInstance(wipe);
            _parsersRepository = await ParsersRepository.GetInstance(_repository);

            _fileSystemObserver = await FileSystemObserver.GetInstance(_repository);

            _searchEngine = await SearchEngine.GetInstance(_repository, _parsersRepository);

            _parserEngine = await ParserEngine.GetInstance(_repository, _parsersRepository);
            _searchEngine.NewGalleriesSelected += (sender, args) => _parserEngine.OnNewGalleriesSelected();
        }
    }
}
