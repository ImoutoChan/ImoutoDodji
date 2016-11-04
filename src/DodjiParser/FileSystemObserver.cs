using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;
using DodjiParser.Models;

namespace DodjiParser
{
    public class FileSystemObserver
    {
        private struct Observer
        {
            public SourceFolder SourceFolder { get; set; }

            public FolderObserver FolderObserver { get; set; }
        }

        private const string PreviewStoragePath = "Thumbs";

        private readonly DataRepository _repository;
        private readonly List<Observer> _fsObservers = new List<Observer>();
        private List<Gallery> _savedGalleries;

        public FileSystemObserver(DataRepository repository)
        {
            _repository = repository;
            Init();
        }

        private async Task Init()
        {
            await LoadGalleries();
            await LoadFolders();
        }

        private async Task LoadGalleries()
        {
            _savedGalleries = await _repository.GetGalleries();
        }

        private async Task LoadFolders()
        {
            var collections = await _repository.GetCollectionsWithFolders();

            foreach (var collection in collections)
            {
                foreach (var collectionSourceFolder in collection.SourceFolders)
                {
                    var di = new DirectoryInfo(collectionSourceFolder.Path);
                    if (!di.Exists)
                    {
                        // TODO log
                        continue;
                    }

                    var fo = new FolderObserver(di, collectionSourceFolder.ObservationType);
                    fo.CurrentStateUpdated += FoOnCurrentStateUpdated;

                    _fsObservers.Add(new Observer {FolderObserver = fo, SourceFolder = collectionSourceFolder});
                }
            }
        }

        private async void FoOnCurrentStateUpdated(object sender, FolderObserver.CurrentStateEventArgs currentStateEventArgs)
        {
            var observer = sender as FolderObserver;
            var galleries = currentStateEventArgs.FileSystemGalleries;

            await ProcessGalleries(_fsObservers.First(x => x.FolderObserver == sender), galleries);
        }

        private async Task ProcessGalleries(Observer observer, List<IFileSystemGallery> galleries)
        {
            var newGalleries = galleries.Where(g => _savedGalleries.Select(sg => sg.Path).Contains(g.Path));

            foreach (var fileSystemGallery in newGalleries)
            {
                await SaveGallery(fileSystemGallery, observer);
            }
        }

        private async Task SaveGallery(IFileSystemGallery fileSystemGallery, Observer observer)
        {
            if (!fileSystemGallery.Exists)
            {
                throw new ArgumentException($"Can't find {fileSystemGallery.Path} gallery.");
            }

            var dbGallery = new Gallery
            {
                Path = fileSystemGallery.Path,
                CollectionId = observer.SourceFolder.CollectionId,
                Size = await fileSystemGallery.GetSize(),
                FilesCount = await fileSystemGallery.GetFileCount(),
                Md5 = await fileSystemGallery.GetMd5(),
                Name = fileSystemGallery.Name,
                PreviewPath = await fileSystemGallery.GeneratePreview(PreviewStoragePath),
                StorageType = fileSystemGallery.StorageType
            };
            fileSystemGallery.Dispose();

            await _repository.AddGallery(dbGallery);
        }
    }
}
