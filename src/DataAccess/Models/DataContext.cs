using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.db");
        }
    }

    public class Namespace
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Tag> Tags { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int NamespaceId { get; set; }
        public Namespace Namespace { get; set; }
    }
}