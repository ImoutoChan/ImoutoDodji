using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DataAccess.Models;

namespace DataAccess.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20161103204133_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-preview1-22509");

            modelBuilder.Entity("DataAccess.Models.BindedTag", b =>
                {
                    b.Property<int>("TagId");

                    b.Property<int>("GalleryId");

                    b.Property<int>("NamespaceId");

                    b.Property<string>("Value");

                    b.HasKey("TagId", "GalleryId", "NamespaceId");

                    b.HasIndex("GalleryId");

                    b.HasIndex("NamespaceId");

                    b.HasIndex("TagId");

                    b.HasIndex("GalleryId", "NamespaceId");

                    b.HasIndex("TagId", "NamespaceId");

                    b.ToTable("BindedTags");
                });

            modelBuilder.Entity("DataAccess.Models.Collection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("DataAccess.Models.DestinationFolder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CollectionId");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CollectionId")
                        .IsUnique();

                    b.ToTable("DestinationFolders");
                });

            modelBuilder.Entity("DataAccess.Models.Gallery", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CollectionId");

                    b.Property<long>("FileSize");

                    b.Property<int>("FilesCount");

                    b.Property<string>("Md5")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Path")
                        .IsRequired();

                    b.Property<string>("PreviewPath")
                        .IsRequired();

                    b.Property<int>("StorageType");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.ToTable("Galleries");
                });

            modelBuilder.Entity("DataAccess.Models.Namespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Namespaces");
                });

            modelBuilder.Entity("DataAccess.Models.SourceFolder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CollectionId");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.ToTable("SourceFolders");
                });

            modelBuilder.Entity("DataAccess.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("DataAccess.Models.BindedTag", b =>
                {
                    b.HasOne("DataAccess.Models.Gallery", "Gallery")
                        .WithMany("BindedTags")
                        .HasForeignKey("GalleryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataAccess.Models.Namespace", "Namespace")
                        .WithMany("BindedTags")
                        .HasForeignKey("NamespaceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataAccess.Models.Tag", "Tag")
                        .WithMany("BindedTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataAccess.Models.DestinationFolder", b =>
                {
                    b.HasOne("DataAccess.Models.Collection", "Collection")
                        .WithOne("DestinationFolder")
                        .HasForeignKey("DataAccess.Models.DestinationFolder", "CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataAccess.Models.Gallery", b =>
                {
                    b.HasOne("DataAccess.Models.Collection", "Collection")
                        .WithMany()
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataAccess.Models.SourceFolder", b =>
                {
                    b.HasOne("DataAccess.Models.Collection", "Collection")
                        .WithMany("SourceFolders")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
