using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DataRepository
    {
        #region Static members

        public static async Task<DataRepository> GetInstance(bool wipe = false)
        {
            var rep = new DataRepository();
            if (wipe)
            {
                await rep.WipeDatabaseAsync();
            }
            else
            {
                await rep.MigrateAsync();
            }
            return rep;
        }

        #endregion

        #region Fields

        private DataCache _cache = new DataCache();

        #endregion

        #region Constructor

        private DataRepository()
        {
        }

        #endregion

        //public async Task<Tag> GetTag(string nameSpaceName, string tagName)
        //{
        //    using (var db = new DataContext())
        //    {
        //        var dbNameSpace = db.Namespaces.FirstOrDefaultAsync(x => x.Name == nameSpaceName);
        //        if (dbNameSpace == null)
        //        {
        //            await CreateNamespace(nameSpaceName, db);
        //        }

        //        var dbTag = db.Tags.FirstOrDefaultAsync(x => x.Name == tagName);
        //        if (dbTag == null)
        //        {
        //            //await CreateTag(tagName, db);
        //        }
        //    }

        //    return null;
        //}

        //#region Private methods

        //private async Task<Namespace> CreateNamespace(string nameSpaceName, DataContext db)
        //{
        //    var nameSpace = await db.Namespaces.AddAsync(new Namespace { Name = nameSpaceName });
        //    return nameSpace.Entity;
        //}

        //private Tag CreateTag(string tagName, DataContext db)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion

        #region Collection Management

        public async Task AddCollection(string name)
        {
            var collection = new Collection {Name = name};
            using (var db = new DataContext())
            {
                var flag = await db.Collections.AnyAsync(x => x.Name == name);
                if (flag)
                {
                    throw new ArgumentException($"Collection with same name ({name}) already exists.");
                }

                await db.Collections.AddAsync(collection);
                await db.SaveChangesAsync();
            }

            await CacheAdd(_cache.Collections, collection);
        }

        public async Task RemoveCollection(int collectionId)
        {
            using (var db = new DataContext())
            {
                var collection = await db.Collections.FindAsync(collectionId);
                if (collection == null)
                {
                    throw new ArgumentException($"Collection ({collectionId}) doesn't exist.");
                }
                
                db.Collections.Remove(collection);
                await db.SaveChangesAsync();
            }

            await CacheRemove(_cache.Collections, collectionId);
        }

        public async Task RenameCollection(int collectionId, string newName)
        {
            Collection colToUpdate;
            using (var db = new DataContext())
            {
                var collection = await db.Collections.FindAsync(collectionId);
                if (collection == null)
                {
                    throw new ArgumentException($"Collection ({collectionId}) doesn't exist.");
                }
                if (newName == collection.Name)
                {
                    return;
                }
                var flag = await db.Collections.AnyAsync(x => x.Name == newName);
                if (flag)
                {
                    throw new ArgumentException($"Collection with same name ({newName}) already exists.");
                }

                collection.Name = newName;
                await db.SaveChangesAsync();
                colToUpdate = collection;
            }

            await CacheUpdate(_cache.Collections, colToUpdate);
        }

        public async Task<IEnumerable<Collection>> GetCollections()
        {
            await FillCache<Collection>();

            return _cache.Collections;
        }
        
        #endregion

        #region Source Folder Management

        public async Task<List<SourceFolder>> GetSourceFolders(int collectionId)
        {
            using (var db = new DataContext())
            {
                var sourceFolders = db.SourceFolders.Where(x => x.CollectionId == collectionId);
                return await sourceFolders.ToListAsync();
            }
        }

        public async Task AddSourceFolder(SourceFolder sourceFolder)
        {
            using (var db = new DataContext())
            {
                await db.SourceFolders.AddAsync(sourceFolder);
                await db.SaveChangesAsync();
            }

            await ReFillCache<Collection>();
            OnCollectionChanged();
        }

        public async Task RemoveSourceFolder(int sourceFolderId)
        {
            using (var db = new DataContext())
            {
                var sourceFolder = await db.SourceFolders.FindAsync(sourceFolderId);
                if (sourceFolder == null)
                {
                    throw new ArgumentException($"Source folder ({sourceFolderId}) doesn't exist.");
                }

                db.SourceFolders.Remove(sourceFolder);
                await db.SaveChangesAsync();
            }


            await ReFillCache<Collection>();
            OnCollectionChanged();
        }

        public async Task UpdateSourceFolder(SourceFolder sourceFolder)
        {
            if (sourceFolder.Id == 0)
            {
                throw new ArgumentException($"Source folder ({sourceFolder.Path}) doesn't exist.");
            }

            using (var db = new DataContext())
            {
                var dbSourceFolder = await db.SourceFolders.FindAsync(sourceFolder.Id);
                if (dbSourceFolder == null)
                {
                    throw new ArgumentException($"Source folder ({sourceFolder.Path}) doesn't exist.");
                }

                dbSourceFolder.Path = sourceFolder.Path;
                dbSourceFolder.KeepRelativePath = sourceFolder.KeepRelativePath;
                dbSourceFolder.ObservationType = sourceFolder.ObservationType;
                
                await db.SaveChangesAsync();
            }


            await ReFillCache<Collection>();
            OnCollectionChanged();
        }

        #endregion

        #region Destination Folder Management

        public async Task<List<DestinationFolder>> GetDestinationFolders(int collectionId)
        {
            using (var db = new DataContext())
            {
                var destinationFolders = db.DestinationFolders.Where(x => x.CollectionId == collectionId);
                return await destinationFolders.ToListAsync();
            }
        }

        public async Task AddDestinationFolder(DestinationFolder destinationFolder)
        {
            using (var db = new DataContext())
            {
                var collection = await db.Collections.FindAsync(destinationFolder.CollectionId);
                if (collection.DestinationFolder != null)
                {
                    // Replace?
                    throw new AggregateException("This collection already contains destination folder");
                }

                await db.DestinationFolders.AddAsync(destinationFolder);
                await db.SaveChangesAsync();
            }

            await ReFillCache<Collection>();

            OnCollectionChanged();
        }

        public async Task RemoveDestinationFolder(int destinationFolderId)
        {
            using (var db = new DataContext())
            {
                var destinationFolder = await db.DestinationFolders.FindAsync(destinationFolderId);
                if (destinationFolder == null)
                {
                    throw new ArgumentException($"Destination folder ({destinationFolderId}) doesn't exist.");
                }

                db.DestinationFolders.Remove(destinationFolder);
                await db.SaveChangesAsync();
            }

            await ReFillCache<Collection>();
            OnCollectionChanged();
        }

        public async Task UpdateDestinationFolder(DestinationFolder destinationFolder)
        {
            if (destinationFolder.Id == 0)
            {
                throw new ArgumentException($"Destination folder ({destinationFolder.Path}) doesn't exist.");
            }

            using (var db = new DataContext())
            {
                var dbDestinationFolder = await db.DestinationFolders.FindAsync(destinationFolder.Id);
                if (dbDestinationFolder == null)
                {
                    throw new ArgumentException($"Destination folder ({destinationFolder.Path}) doesn't exist.");
                }

                dbDestinationFolder.Path = destinationFolder.Path;

                await db.SaveChangesAsync();
            }

            await ReFillCache<Collection>();
            OnCollectionChanged();
        }

        #endregion

        #region Gallery Management

        public async Task AddGallery(Gallery gallery)
        {
            using (var db = new DataContext())
            {
                var flag = await db.Galleries.AnyAsync(x => x.Path == gallery.Path);
                if (flag)
                {
                    throw new ArgumentException($"Gallery with same path ({gallery.Path}) already exists.");
                }

                await db.Galleries.AddAsync(gallery);
                await db.SaveChangesAsync();

                // adding to parsing table
                var parserState = new ParsingState
                {
                    State = GalleryState.Init,
                    DateTimeCreated = DateTime.Now,
                    DateTimeUpdated = DateTime.Now,
                    GalleryId = gallery.Id
                };

                await db.ParsingStates.AddAsync(parserState);
                await db.SaveChangesAsync();
            }

            await CacheAdd(_cache.Galleries, gallery);
        }

        public async Task<IEnumerable<Gallery>> GetGalleries()
        {
            await FillCache<Gallery>();

            return _cache.Galleries;
        }

        #endregion

        #region ParsingStates Management
        
        public async Task<IEnumerable<ParsingState>> GetParsingStates(Expression<Func<ParsingState, bool>> predicate)
        {
            using (var db = new DataContext())
            {
                var pss = await db.ParsingStates.Where(predicate).ToListAsync();

                foreach (var parsingState in pss)
                {
                    parsingState.Gallery = (await GetGalleries()).First(x => x.Id == parsingState.GalleryId);
                }

                return pss;
            }
        }

        public async Task SetParsingStatus(int parsingStateId, GalleryState galleryState, string errorString = null)
        {
            using (var db = new DataContext())
            {
                var ps = await db.ParsingStates.FindAsync(parsingStateId);
                ps.State = galleryState;
                ps.DateTimeUpdated = DateTime.Now;
                ps.ErrorString = errorString;
                await db.SaveChangesAsync();
            }
        }

        #endregion

        #region Cache Management

        private async Task CacheAdd<T>(IList<T> cacheEntries, T entry)
        {
            await FillCache<T>();

            lock (cacheEntries)
            {
                cacheEntries.Add(entry);
            }

            OnEntryChanged<T>();
        }

        private async Task CacheRemove<T>(IList<T> cacheEntries, int id) where T : EntityBase
        {
            await FillCache<T>();

            lock (cacheEntries)
            {
                cacheEntries.Remove(cacheEntries.First(x => x.Id == id));
            }

            OnEntryChanged<T>();
        }

        private async Task CacheUpdate<T>(IList<T> cacheEntries, T entry) where T : EntityBase
        {
            await FillCache<T>();

            lock (cacheEntries)
            {
                cacheEntries.Remove(cacheEntries.First(x => x.Id == entry.Id));
                cacheEntries.Add(entry);
            }

            OnEntryChanged<T>();
        }

        private async Task FillCache<T>()
        {
            if (typeof(T) == typeof(Collection))
            {
                await FillCollectionCache();
            }
            else // if (typeof(T) == typeof(Gallery)
            {
                await FillGalleryCache();
            }
        }

        private async Task ReFillCache<T>()
        {
            if (typeof(T) == typeof(Collection))
            {
                _cache.IsCollectionsInitialized = false;
                await FillCollectionCache();
            }
            else // if (typeof(T) == typeof(Gallery)
            {
                _cache.IsGalleriesInitialized = false;
                await FillGalleryCache();
            }
        }

        private void OnEntryChanged<T>()
        {
            if (typeof(T) == typeof(Collection))
            {
                OnCollectionChanged();
            }
            else // if (typeof(T) == typeof(Gallery)
            {
                OnGalleriesChanged();
            }
        }

        private async Task FillCollectionCache()
        {
            if (!_cache.IsCollectionsInitialized)
            {
                List<Collection> collections;
                using (var db = new DataContext())
                {
                    collections = await db.Collections
                        .Include(x => x.SourceFolders)
                        .Include(x => x.DestinationFolder)
                        .ToListAsync();
                }

                lock (_cache.Collections)
                {
                    if (!_cache.IsCollectionsInitialized)
                    {
                        _cache.Collections.Clear();
                        foreach (var collection in collections)
                        {
                            _cache.Collections.Add(collection);
                        }
                    }
                    _cache.IsCollectionsInitialized = true;
                }
            }
        }

        private async Task FillGalleryCache()
        {
            if (!_cache.IsGalleriesInitialized)
            {
                List<Gallery> gals;
                using (var db = new DataContext())
                {
                    gals = await db.Galleries.ToListAsync();
                }

                lock (_cache.Galleries)
                {
                    if (!_cache.IsGalleriesInitialized)
                    {
                        _cache.Galleries.Clear();
                        foreach (var gallery in gals)
                        {
                            _cache.Galleries.Add(gallery);
                        }
                    }
                    _cache.IsGalleriesInitialized = true;
                }
            }
        }

        #endregion

        #region Database engine

        private async Task WipeDatabaseAsync()
        {
            using (var db = new DataContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.MigrateAsync();
            }
        }

        private void WipeDatabase()
        {
            using (var db = new DataContext())
            {
                db.Database.EnsureDeleted();
                db.Database.Migrate();
            }
        }
        
        private async Task MigrateAsync()
        {
            using (var db = new DataContext())
            {
                await db.Database.MigrateAsync();
            }
        }

        #endregion

        #region Events

        public event EventHandler CollectionChanged;

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler GalleriesChanged;

        protected virtual void OnGalleriesChanged()
        {
            GalleriesChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
