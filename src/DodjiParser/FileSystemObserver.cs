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
        #region Static members

        public static async Task<FileSystemObserver> GetInstance(DataRepository repository)
        {
            var fso = new FileSystemObserver(repository);

            await fso.Initialize();

            return fso;
        }
        

        #endregion

        #region Types

        private struct Observer
        {
            public SourceFolder SourceFolder { get; set; }

            public FolderObserver FolderObserver { get; set; }
        }

        #endregion

        #region Fields

        private const string _previewStoragePath = "Thumbs";

        private readonly DataRepository _repository;
        private readonly List<Observer> _fsObservers = new List<Observer>();
        private List<Gallery> _savedGalleries;
        private List<IFileSystemGallery> _processingGalleries = new List<IFileSystemGallery>();

        #endregion

        #region Constructors

        private FileSystemObserver(DataRepository repository)
        {
            _repository = repository;
            _repository.CollectionChanged += _repository_CollectionChanged;
            _repository.GalleriesChanged += _repository_GalleriesChanged;
        }

        #endregion

        #region Private methods

        private async void _repository_GalleriesChanged(object sender, EventArgs e)
        {
            await ReloadGalleries();
        }

        private async void _repository_CollectionChanged(object sender, EventArgs e)
        {
            await ReloadFolders();
        }

        private async Task Initialize()
        {
            await ReloadGalleries();
            await ReloadFolders();
        }

        private async Task ReloadGalleries()
        {
            _savedGalleries = (await _repository.GetGalleries()).ToList();
        }

        private async Task ReloadFolders()
        {
            foreach (var fsObserver in _fsObservers)
            {
                fsObserver.FolderObserver.CurrentStateUpdated -= FoOnCurrentStateUpdated;
                fsObserver.FolderObserver.Dispose();
            }
            _fsObservers.Clear();

            var collections = (await _repository.GetCollections()).ToList();

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

        private async void FoOnCurrentStateUpdated(object sender,
            FolderObserver.CurrentStateEventArgs currentStateEventArgs)
        {

            var observer = sender as FolderObserver;
            var galleries = currentStateEventArgs.FileSystemGalleries;
            
            await ProcessGalleries(_fsObservers.First(x => x.FolderObserver == sender), galleries);
        }

        private async Task ProcessGalleries(Observer observer, List<IFileSystemGallery> galleries)
        {
            var newGalleries = FilterGalleries(galleries, observer, _savedGalleries.Select(sg => sg.Path).ToList()).ToList();
            lock (_processingGalleries)
            {
                newGalleries = FilterGalleries(newGalleries, observer, _processingGalleries.Select(sg => sg.Path).ToList()).ToList();
                _processingGalleries.AddRange(newGalleries);
            }
            
            foreach (var fileSystemGallery in newGalleries)
            {
                try
                {
                    await SaveGallery(fileSystemGallery, observer);
                }
                catch (Exception ex)
                {
                    // TODO log
                }
                finally
                {
                    lock (_processingGalleries)
                    {
                        _processingGalleries.Remove(fileSystemGallery);
                        fileSystemGallery.Dispose();
                    }
                }
            }

            if (newGalleries.Count > 0)
            {
                OnNewGalleriesAppeared();
            }
        }

        private IEnumerable<IFileSystemGallery> FilterGalleries(IEnumerable<IFileSystemGallery> newGalleries, Observer observer, List<string> filterStrings)
        {
            foreach (var gallery in newGalleries)
            {
                var relativePath = observer.SourceFolder.KeepRelativePath ? gallery.Path.Substring(observer.SourceFolder.Path.Length) : gallery.Path.Split(new [] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries).Last();
                if (!filterStrings.Any(fs => fs.EndsWith(relativePath)))
                {
                    yield return gallery;
                }
            }
        }

        private async Task SaveGallery(IFileSystemGallery fileSystemGallery, Observer observer)
        {
            if (!fileSystemGallery.Exists)
            {
                throw new ArgumentException($"Can't find {fileSystemGallery.Path} gallery.");
            }

            if (observer.SourceFolder.Collection.DestinationFolder != null)
            {
                await fileSystemGallery.Move(observer.SourceFolder.Path, observer.SourceFolder.Collection.DestinationFolder.Path, observer.SourceFolder.KeepRelativePath);
            }
            
            var dbGallery = new Gallery
            {
                Path = fileSystemGallery.Path,
                CollectionId = observer.SourceFolder.CollectionId,
                Size = await fileSystemGallery.GetSize(),
                FilesCount = await fileSystemGallery.GetFileCount(),
                Md5 = await fileSystemGallery.GetMd5(),
                Name = fileSystemGallery.Name,
                PreviewPath = await fileSystemGallery.GeneratePreview(_previewStoragePath),
                StorageType = fileSystemGallery.StorageType
            };

            await _repository.AddGallery(dbGallery);
        }

        #endregion

        public event EventHandler NewGalleriesAppeared;

        protected virtual void OnNewGalleriesAppeared()
        {
            NewGalleriesAppeared?.Invoke(this, EventArgs.Empty);
        }
    }
}
