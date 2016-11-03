using System.IO;

namespace DodjiParser.Models
{
    public abstract class FileSystemGallery
    {
        public string Path { get; set; }
    }

    public class FolderFileSystemGallery : FileSystemGallery
    {
        private readonly DirectoryInfo _directoryInfo;

        public FolderFileSystemGallery(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
            Path = _directoryInfo.FullName;
        }
    }

    public class ArchiveFileSystemGallery : FileSystemGallery
    {
        private readonly FileInfo _archiveFileInfo;

        public ArchiveFileSystemGallery(FileInfo archiveFileInfo)
        {
            _archiveFileInfo = archiveFileInfo;
            Path = _archiveFileInfo.FullName;
        }
    }
}