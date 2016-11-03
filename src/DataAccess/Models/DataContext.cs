using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models
{
    public class DataContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.db");
        }
    }

    public class EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }

    public class Namespace : EntityBase
    {
        public string Name { get; set; }
    }

    public class Tag : EntityBase
    {
        public string Name { get; set; }
    }
}