using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using ImageSharp;
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

        Task Move(string sourceFolderPath, string destinationFolderPath, bool keepRelativePath);
    }

    public abstract class FileSystemGallery
    {
        protected string CreateMd5ForFolder(List<FileInfo> files)
        {
            List<string> hashes = new List<string>();

            foreach (var file in files)
            {
                var md5String = GetMd5ForFile(file);
                hashes.Add(md5String);
            }

            var inputBytes = Encoding.UTF8.GetBytes(String.Join(";", hashes));

            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(inputBytes)).Replace("-", "").ToLower();
            }
        }

        private string GetMd5ForFile(FileInfo file)
        {
            using (MD5 md5 = MD5.Create())
            using (var stream = new FileStream(file.FullName, FileMode.Open))
            {

                var md5String = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                return md5String;
            }
        }

        protected async Task<string> GeneratePreview(FileInfo fileInfo, string previewStoragePath, string md5)
        {
            return await Task.Run(() =>
            {
                var currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
                var previewDir = currentFolder.GetDirectories().FirstOrDefault(x => x.Name == previewStoragePath);
                var previewDirInfo = previewDir ?? currentFolder.CreateSubdirectory(previewStoragePath);

                var previewFilename = Path.Combine(previewDirInfo.FullName, md5 + ".jpg");

                var previewFileInfo = new FileInfo(previewFilename);
                if (previewFileInfo.Exists)
                {
                    return previewFileInfo.FullName;
                }

                using (FileStream stream = File.OpenRead(fileInfo.FullName))
                using (FileStream output = File.OpenWrite(previewFilename))
                {
                    Image image = new Image(stream);
                    image.Resize(Convert.ToInt32((image.Width / (double)image.Height) * 450), 450)
                         .Save(output);
                }
                return previewFilename;
            });
        }
    }


    public class FolderFileSystemGallery : FileSystemGallery, IFileSystemGallery
    {
        private DirectoryInfo _directoryInfo;
        private List<FileInfo> _images;
        private string _md5 = null;

        public FolderFileSystemGallery(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
            ReloadImages();
        }

        private void ReloadImages()
        {
            _images =
                SupportedExtensions.GetFilesWithExtensions(_directoryInfo, SupportedExtensions.GetImages()).ToList();
            if (!_images.Any())
            {
                throw new ArgumentException(
                    $"Directory ({_directoryInfo.Name}) should contain at least one supported image file.");
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
            if (_md5 != null)
            {
                return _md5;
            }

            await Task.Run(() =>
            {
                _md5 = CreateMd5ForFolder(_images);
            });

            return _md5;
        }

        public async Task<long> GetSize()
        {
            return await Task.Run(() => _images.Select(i => i.Length).Aggregate((a, i) => a + i));
        }

        public async Task<string> GeneratePreview(string previewStoragePath)
        {
            return await base.GeneratePreview(_images.First(), previewStoragePath, await GetMd5());
        }

        public async Task Move(string sourceFolderPath, string destinationFolderPath, bool keepRelativePath)
        {
            await Task.Run(() =>
            {
                var subPath = _directoryInfo.Parent.FullName.Substring(sourceFolderPath.Length);
                if (!String.IsNullOrWhiteSpace(subPath) && keepRelativePath)
                {
                    destinationFolderPath = System.IO.Path.Combine(destinationFolderPath, subPath.Substring(1),
                        _directoryInfo.Name);
                }
                else
                {
                    destinationFolderPath = System.IO.Path.Combine(destinationFolderPath, _directoryInfo.Name);
                }

                var dirInfo = new DirectoryInfo(destinationFolderPath);

                if (!dirInfo.Parent.Exists)
                {
                    dirInfo.Parent.Create();
                }

                _directoryInfo.MoveTo(dirInfo.FullName);

                if (!dirInfo.Exists)
                {
                    throw new Exception("Directory was not be moved.");
                }
                _directoryInfo = dirInfo;
                ReloadImages();
            });
        }

        public void Dispose()
        { }
    }

    public class ArchiveFileSystemGallery : FileSystemGallery, IFileSystemGallery
    {
        private const string TempArchivePath = "Temp";
        private List<FileInfo> _images;
        private readonly object _imagesLock = new object();
        private string _md5 = null;

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

            if (_md5 != null)
            {
                return _md5;
            }

            await Task.Run(() =>
            {
                _md5 = CreateMd5ForFolder(_images);
            });

            return _md5;
        }

        public async Task<string> GeneratePreview(string previewStoragePath)
        {
            await LoadImages();

            return await GeneratePreview(_images.First(), previewStoragePath, await GetMd5());
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
                SupportedExtensions.GetFilesWithExtensions(archiveTempDirInfo, SupportedExtensions.GetImages(), SearchOption.AllDirectories)
                    .ToList();

            if (!_images.Any())
            {
                throw new Exception($"Archive {_archiveFileInfo.FullName} doesn't contain any supported images.");
            }
        }

        public async Task Move(string sourceFolderPath, string destinationFolderPath, bool keepRelativePath)
        {
            await Task.Run(() =>
            {
                var subPath = _archiveFileInfo.Directory.FullName.Substring(sourceFolderPath.Length);
                if (!String.IsNullOrWhiteSpace(subPath) && keepRelativePath)
                {
                    destinationFolderPath = System.IO.Path.Combine(destinationFolderPath, subPath.Substring(1));
                }

                var dirInfo = new DirectoryInfo(destinationFolderPath);

                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                _archiveFileInfo.MoveTo(System.IO.Path.Combine(dirInfo.FullName, _archiveFileInfo.Name));
            });
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