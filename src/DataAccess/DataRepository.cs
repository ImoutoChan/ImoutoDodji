using System;
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

        public async Task<Tag> GetTag(string nameSpaceName, string tagName)
        {
            using (var db = new DataContext())
            {
                var dbNameSpace = db.Namespaces.FirstOrDefaultAsync(x => x.Name == nameSpaceName);
                if (dbNameSpace == null)
                {
                    await CreateNamespace(nameSpaceName, db);
                }

                var dbTag = db.Tags.FirstOrDefaultAsync(x => x.Name == tagName);
                if (dbTag == null)
                {
                    //await CreateTag(tagName, db);
                }
            }

            return null;
        }

        #region Private methods
        
        private async Task<Namespace> CreateNamespace(string nameSpaceName, DataContext db)
        {
            var nameSpace = await db.Namespaces.AddAsync(new Namespace { Name = nameSpaceName });
            return nameSpace.Entity;
        }

        private Tag CreateTag(string tagName, DataContext db)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
