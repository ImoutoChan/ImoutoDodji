using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using SharpCompress.Archives;
using SharpCompress.Readers;

namespace DodjiParser.Models
{
    public interface IFileSystemGallery : IDisposable
    {
        string Path { get; }

        bool Exists { get; }

        string Name { get; }

        StorageType StorageType { get; }

        Task<int> GetFileCount();

        Task<string> GetMd5();

        Task<string> GeneratePreview(string previewStoragePath);

        Task<long> GetSize();
    }

    public abstract class FileSystemGallery
    {
        protected string GetImageMd5(Bitmap image)
        {
            byte[] imgBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                imgBytes = ms.ToArray();
            }

            var hash = MD5.Create().ComputeHash(imgBytes);
            var imageMD5 = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return imageMD5;
        }

        protected static Bitmap ResizeImage(Image image, int height)
        {
            var width = Convert.ToInt32(image.HorizontalResolution / image.VerticalResolution) * height;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        protected string CreateMd5ForFolder(List<FileInfo> files)
        {
            List<string> hashes = new List<string>();

            using (MD5 md5 = MD5.Create())
            {
                foreach (var file in files)
                {
                    var stream = new FileStream(file.FullName, FileMode.Open);
                    hashes.Add(BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }
                var inputBytes = Encoding.UTF8.GetBytes(String.Join(";", hashes));

                return BitConverter.ToString(md5.ComputeHash(inputBytes)).Replace("-", "").ToLower();
            }
        }

        protected async Task<string> GeneratePreview(FileInfo fileInfo, string previewStoragePath)
        {
            return await Task.Run(() =>
            {
                var preview = ResizeImage(Bitmap.FromFile(fileInfo.FullName), 450);

                var imageMD5 = GetImageMd5(preview);

                var currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
                var previewDir = currentFolder.GetDirectories().FirstOrDefault(x => x.Name == previewStoragePath);
                var previewDirInfo = previewDir ?? currentFolder.CreateSubdirectory(previewStoragePath);

                var filename = previewDirInfo.FullName + "@\\" + imageMD5 + ".jpg";
                preview.Save(filename, ImageFormat.Jpeg);

                return filename;
            });
        }
    }


    public class FolderFileSystemGallery : FileSystemGallery, IFileSystemGallery
    {
        private readonly DirectoryInfo _directoryInfo;
        private readonly List<FileInfo> _images;

        public FolderFileSystemGallery(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
            _images =
                SupportedExtensions.GetFilesWithExtensions(_directoryInfo, SupportedExtensions.GetImages()).ToList();
            if (!_images.Any())
            {
                throw new ArgumentException(
                    $"Directory ({directoryInfo.Name}) should contain at least one supported image file.");
            }
        }

        public string Path => _directoryInfo.FullName;

        public bool Exists => _directoryInfo.Exists;

        public string Name => _directoryInfo.Name;

        public StorageType StorageType => StorageType.Folder;
        
        public async Task<int> GetFileCount()
        {
            return _images.Count();
        }

        public async Task<string> GetMd5()
        {
            return await Task.Run(() => CreateMd5ForFolder(_images));
        }

        public async Task<long> GetSize()
        {
            return await Task.Run(() => _images.Select(i => i.Length).Aggregate((a, i) => a + i));
        }

        public async Task<string> GeneratePreview(string previewStoragePath)
        {
            return await GeneratePreview(_images.First(), previewStoragePath);
        }

        public void Dispose()
        { }
    }

    public class ArchiveFileSystemGallery : FileSystemGallery, IFileSystemGallery
    {
        private const string TempArchivePath = "Temp";
        private List<FileInfo> _images;
        private readonly object _imagesLock = new object();

        private readonly FileInfo _archiveFileInfo;

        public ArchiveFileSystemGallery(FileInfo archiveFileInfo)
        {
            _archiveFileInfo = archiveFileInfo;
        }

        public string Path => _archiveFileInfo.FullName;

        public bool Exists => _archiveFileInfo.Exists;

        public string Name => System.IO.Path.GetFileNameWithoutExtension(_archiveFileInfo.Name);

        public StorageType StorageType => StorageType.Archive;

        public async Task<int> GetFileCount()
        {
            await LoadImages();

            return _images.Count();
        }

        public async Task<string> GetMd5()
        {
            await LoadImages();

            return await Task.Run(() => CreateMd5ForFolder(_images));
        }

        public async Task<string> GeneratePreview(string previewStoragePath)
        {
            await LoadImages();

            return await GeneratePreview(_images.First(), previewStoragePath);
        }

        public async Task<long> GetSize()
        {
            await LoadImages();
            return await Task.Run(() => _images.Select(i => i.Length).Aggregate((a, i) => a + i));

        }

        private async Task LoadImages()
        {
            if (_images == null)
            {
                await Task.Run(() =>
                {
                    lock (_imagesLock)
                    {
                        if (_images == null)
                        {
                            LazyExtract();
                        }
                    }
                });
            }
        }

        private void LazyExtract()
        {
            var archive = ArchiveFactory.Open(_archiveFileInfo.FullName);

            var currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            var tempDir = currentFolder.GetDirectories().FirstOrDefault(x => x.Name == TempArchivePath);
            var tempDirInfo = tempDir ?? currentFolder.CreateSubdirectory(TempArchivePath);

            var archiveTempDir = tempDirInfo.GetDirectories().FirstOrDefault(x => x.Name == Name);
            var archiveTempDirInfo = archiveTempDir ?? tempDirInfo.CreateSubdirectory(Name);

            foreach (var file in archiveTempDirInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                    continue;

                entry.WriteToDirectory(archiveTempDirInfo.FullName,
                    new ExtractionOptions {ExtractFullPath = true, Overwrite = true});
            }

            _images =
                SupportedExtensions.GetFilesWithExtensions(archiveTempDirInfo, SupportedExtensions.GetImages())
                    .ToList();
        }
        
        public void Dispose()
        {
            try
            {
                var currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
                var tempDir = currentFolder.GetDirectories().FirstOrDefault(x => x.Name == TempArchivePath);

                if (tempDir == null)
                {
                    return;
                }

                var archiveTempDir = tempDir.GetDirectories().FirstOrDefault(x => x.Name == Name);
                if (archiveTempDir == null)
                {
                    return;
                }

                foreach (var file in archiveTempDir.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Delete();
                }
                foreach (
                    var dir in
                    archiveTempDir.GetDirectories("*", SearchOption.AllDirectories)
                        .OrderByDescending(x => x.FullName.Length))
                {
                    dir.Delete();
                }
                archiveTempDir.Delete();
            }
            catch (Exception ex)
            {
                // TODO log
            }
        }
    }
}