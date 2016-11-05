using System.Threading.Tasks;
using DataAccess;

namespace DodjiParser
{
    public class Initializer
    {
        private DataRepository _repository;
        private FileSystemObserver _fileSystemObserver;
        
        public Initializer(bool wipe = false)
        {
            Init(wipe);
        }

        public DataRepository Repository => _repository;

        public void Init(bool wipe)
        {
            DataRepository.WipeDatabase();
            _repository = new DataRepository();
            _fileSystemObserver = new FileSystemObserver(_repository);
        }
    }
}
