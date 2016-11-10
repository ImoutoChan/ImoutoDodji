using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InfoParser.LocalDatabase
{
    public class LocalDbSourceContext : DbContext
    {
        public virtual DbSet<ViewerGallery> ViewerGallery { get; set; }
        public virtual DbSet<ViewerGalleryTags> ViewerGalleryTags { get; set; }
        public virtual DbSet<ViewerTag> ViewerTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlite(@"Filename=exhentai.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ViewerGallery>(entity =>
            {
                entity.ToTable("viewer_gallery");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("integer");

                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .HasColumnName("comment")
                    .HasColumnType("text");

                entity.Property(e => e.CreateDate)
                    .IsRequired()
                    .HasColumnName("create_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.DlType)
                    .IsRequired()
                    .HasColumnName("dl_type")
                    .HasColumnType("varchar(40)");

                entity.Property(e => e.Expunged)
                    .HasColumnName("expunged")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Filecount)
                    .HasColumnName("filecount")
                    .HasColumnType("integer");

                entity.Property(e => e.Filesize)
                    .HasColumnName("filesize")
                    .HasColumnType("integer");

                entity.Property(e => e.Fjord)
                    .IsRequired()
                    .HasColumnName("fjord")
                    .HasColumnType("bool");

                entity.Property(e => e.Gid)
                    .IsRequired()
                    .HasColumnName("gid")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Hidden)
                    .IsRequired()
                    .HasColumnName("hidden")
                    .HasColumnType("bool");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime");

                entity.Property(e => e.Posted)
                    .HasColumnName("posted")
                    .HasColumnType("datetime");

                entity.Property(e => e.Public)
                    .IsRequired()
                    .HasColumnName("public")
                    .HasColumnType("bool");

                entity.Property(e => e.Rating)
                    .HasColumnName("rating")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.TitleJpn)
                    .HasColumnName("title_jpn")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Uploader)
                    .HasColumnName("uploader")
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<ViewerGalleryTags>(entity =>
            {
                entity.ToTable("viewer_gallery_tags");

                entity.HasIndex(e => e.GalleryId)
                    .HasName("viewer_gallery_tags_6d994cdb");

                entity.HasIndex(e => e.TagId)
                    .HasName("viewer_gallery_tags_76f094bc");

                entity.HasIndex(e => new { e.GalleryId, e.TagId })
                    .HasName("viewer_gallery_tags_gallery_id_fca356e6_uniq")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("integer");

                entity.Property(e => e.GalleryId)
                    .HasColumnName("gallery_id")
                    .HasColumnType("integer");

                entity.Property(e => e.TagId)
                    .HasColumnName("tag_id")
                    .HasColumnType("integer");

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.ViewerGalleryTags)
                    .HasForeignKey(d => d.GalleryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.ViewerGalleryTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ViewerTag>(entity =>
            {
                entity.ToTable("viewer_tag");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("integer");

                entity.Property(e => e.CreateDate)
                    .IsRequired()
                    .HasColumnName("create_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Scope)
                    .IsRequired()
                    .HasColumnName("scope")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("varchar(50)");
            });
        }
    }
}