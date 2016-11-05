﻿using System.Threading.Tasks;
using DataAccess;

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
        
        private DodjiService()
        {
        }

        public DataRepository Repository => _repository;

        private async Task Initialize(bool wipe)
        {
            _repository = await DataRepository.GetInstance(wipe);
            _fileSystemObserver = await FileSystemObserver.GetInstance(_repository);
        }
    }
}
