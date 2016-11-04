using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DataRepository
    {
        public DataRepository()
        {
            using (var db = new DataContext())
            {
                db.Database.Migrate();
            }
        }

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

        public async Task<List<Collection>> GetCollections()
        {
            using (var db = new DataContext())
            {
                var collections = await db.Collections.ToListAsync();
                return collections;
            }
        }

        public async Task AddCollection(string name)
        {
            using (var db = new DataContext())
            {
                var flag = await db.Collections.AnyAsync(x => x.Name == name);
                if (flag)
                {
                    throw new ArgumentException($"Collection with same name ({name}) already exists.");
                }

                await db.Collections.AddAsync(new Collection {Name = name});
                await db.SaveChangesAsync();
            }
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
        }

        public async Task RenameCollection(int collectionId, string newName)
        {
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
            }
        }

        public async Task<List<Collection>> GetCollectionsWithFolders()
        {
            using (var db = new DataContext())
            {
                var collections = await db.Collections
                    .Include(x => x.SourceFolders)
                    .Include(x => x.DestinationFolder)
                    .ToListAsync();
                return collections;
            }
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
                await db.DestinationFolders.AddAsync(destinationFolder);
                await db.SaveChangesAsync();
            }
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
        }

        #endregion


        public async Task AddGallery(Gallery gallery)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Gallery>> GetGalleries()
        {
            throw new NotImplementedException();
        }
    }
}
