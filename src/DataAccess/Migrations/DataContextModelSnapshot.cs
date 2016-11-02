using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DataAccess.Models;

namespace DataAccess.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-preview1-22509");

            modelBuilder.Entity("DataAccess.Models.Namespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Namespaces");
                });

            modelBuilder.Entity("DataAccess.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("NamespaceId");

                    b.HasKey("Id");

                    b.HasIndex("NamespaceId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("DataAccess.Models.Tag", b =>
                {
                    b.HasOne("DataAccess.Models.Namespace", "Namespace")
                        .WithMany("Tags")
                        .HasForeignKey("NamespaceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
