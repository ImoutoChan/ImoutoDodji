using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class DataContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        public DbSet<Namespace> Namespaces { get; set; }

        public DbSet<DestinationFolder> DestinationFolders { get; set; }

        public DbSet<SourceFolder> SourceFolders { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<Gallery> Galleries { get; set; } 

        public DbSet<BindedTag> BindedTags { get; set; } 

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
        }
    }

    public class EntityBase
    {
        public int Id { get; set; }
    }

    public class Namespace : EntityBase
    {
        [Required]
        public string Name { get; set; }


        public List<BindedTag> BindedTags { get; set; }
    }

    public class Tag : EntityBase
    {
        [Required]
        public string Name { get; set; }


        public List<BindedTag> BindedTags { get; set; }
    }

    public class DestinationFolder : EntityBase
    {
        [Required]
        public string Path { get; set; }



        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }

    public class SourceFolder : EntityBase
    {
        [Required]
        public string Path { get; set; }



        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }

    public class Collection : EntityBase
    {
        [Required]
        public string Name { get; set; }



        public List<SourceFolder> SourceFolders { get; set; } 

        public DestinationFolder DestinationFolder { get; set; }
    }

    public enum StorageType
    {
        Archive,
        Folder
    }

    public class Gallery : EntityBase
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public int FilesCount { get; set; }

        [Required]
        public string PreviewPath { get; set; }

        [Required]
        public string Md5 { get; set; }

        [Required]
        public StorageType StorageType { get; set; }


        public List<BindedTag> BindedTags { get; set; }

        [Required]
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
    }

    public class BindedTag
    {
        public string Value { get; set; }



        [Required]
        public int GalleryId { get; set; }

        public Gallery Gallery { get; set; }

        [Required]
        public int TagId { get; set; }

        public Tag Tag { get; set; }

        [Required]
        public int NamespaceId { get; set; }

        public Namespace Namespace { get; set; }
    }
}