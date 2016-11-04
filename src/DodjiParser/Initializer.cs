using System.Threading.Tasks;
using DataAccess;

namespace DodjiParser
{
    public class Initializer
    {
        private DataRepository _repository;
        private FileSystemObserver _fileSystemObserver;

        public Initializer()
        {
            Init();
        }

        private async Task Init()
        {
            _repository = new DataRepository();
            _fileSystemObserver = new FileSystemObserver(_repository);
        }
    }
}
