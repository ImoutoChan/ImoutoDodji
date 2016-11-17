using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models
{
    public class DataContext : DbContext
    {
        #region Constructor

        public DataContext()
        {
            Database.Migrate();
        }

        #endregion

        #region DbSets

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Namespace> Namespaces { get; set; }

        public DbSet<DestinationFolder> DestinationFolders { get; set; }

        public DbSet<SourceFolder> SourceFolders { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<Gallery> Galleries { get; set; } 

        public DbSet<BindedTag> BindedTags { get; set; }

        public DbSet<Metadata> Metadata { get; set; }

        public DbSet<PandaMetadata> PandaMetadata { get; set; }

        public DbSet<ParsingState> ParsingStates { get; set; }

        public DbSet<SearchResult> SearchResults { get; set; }

        #endregion

        #region Configuration

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Namespace>()
                .HasIndex(ns => ns.Name)
                .IsUnique(true);
            modelBuilder.Entity<Tag>()
                .HasIndex(ns => ns.Name)
                .IsUnique(true);

            modelBuilder.Entity<BindedTag>()
                .HasIndex(bt => bt.GalleryId);
            modelBuilder.Entity<BindedTag>()
                .HasIndex(bt => bt.TagId);
            modelBuilder.Entity<BindedTag>()
                .HasIndex(bt => bt.NamespaceId);
            modelBuilder.Entity<BindedTag>()
                .HasIndex(bt => new { bt.TagId, bt.NamespaceId});
            modelBuilder.Entity<BindedTag>()
                .HasIndex(bt => new { bt.GalleryId, bt.NamespaceId });
            modelBuilder.Entity<BindedTag>()
                .HasKey(bt => new {bt.TagId, bt.GalleryId, bt.NamespaceId});

            modelBuilder.Entity<ParsingState>()
                .HasIndex(ps => new { ps.State });
        }

        #endregion
    }
}