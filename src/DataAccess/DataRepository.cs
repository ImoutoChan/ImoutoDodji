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

        public async Task<Tag> GetTag(string nameSpace, string name)
        {
            
        }
    }
}
