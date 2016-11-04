using DodjiParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DodjiParser
{
    /// <summary>
    /// Folder observer for files [recursive]
    /// Folder observer for folders [recursive]
    /// 
    /// Structure types:
    /// 
    /// *FilesNonRecursive*
    /// 
    /// [OBSERVED FOLDER]
    /// -- dodji_file1.zip
    /// -- dodji_file2.rar
    ///
    /// *FilesRecursive*
    /// 
    /// [OBSERVED FOLDER]
    /// -- dodji_file1.zip
    /// -- dodji_file2.rar
    /// -- [FOLDER1]
    /// -- -- dodji_file3.zip
    /// -- -- dodji_file4.rar
    /// -- [FOLDER2]
    /// -- -- dodji_file5.zip
    /// -- -- dodji_file6.rar
    /// -- -- [FOLDER3]
    /// -- -- -- dodji_file7.zip
    /// -- -- -- dodji_file8.rar
    /// 
    /// *FoldersNonRecursive*
    /// 
    /// [OBSERVED FOLDER]
    /// -- [dodji_folder1]
    /// -- -- dodji_file1.jpg
    /// -- -- dodji_file2.png
    /// -- -- dodji_file3.gif
    /// -- -- dodji_file4.jpeg
    /// -- [dodji_folder2]
    /// -- -- dodji_file1.jpg
    /// -- -- dodji_file2.png
    /// -- -- dodji_file3.gif
    /// -- -- dodji_file4.jpeg
    /// -- [dodji_folder3]
    /// -- -- dodji_file1.jpg
    /// -- -- dodji_file2.png
    /// -- -- dodji_file3.gif
    /// -- -- dodji_file4.jpeg
    /// 
    /// *FoldersRecursive*
    /// Dodji folders shouldn't contain any other folders and should contain at least one image file.
    /// 
    /// [OBSERVED FOLDER]
    /// -- [dodji_folder1]
    /// -- -- dodji_file1.jpg
    /// -- -- dodji_file2.png
    /// -- -- dodji_file3.gif
    /// -- -- dodji_file4.jpeg
    /// -- [FOLDER1]
    /// -- -- [dodji_folder2]
    /// -- -- -- dodji_file1.jpg
    /// -- -- -- dodji_file2.png
    /// -- -- -- dodji_file3.gif
    /// -- -- -- dodji_file4.jpeg
    /// -- -- [FOLDER1]
    /// -- -- -- [dodji_folder3]
    /// -- -- -- -- dodji_file1.jpg
    /// -- -- -- -- dodji_file2.png
    /// -- -- -- -- dodji_file3.gif
    /// -- -- -- -- dodji_file4.jpeg
    /// </summary>
    public class FolderObserver
    {
        #region Types

        private enum SupportedArchiveExtensions
        {
            Zip,
            Rar,
            Cbz,
            Cbr
        }

        private IEnumerable<string> GetSupportedArchiveExtensions()
        {
            foreach (var name in Enum.GetNames(typeof(SupportedArchiveExtensions)))
            {
                yield return name;
            }
        }

        private enum SupportedArchiveEntryExtensions
        {
            jpg,
            png
        }

        private IEnumerable<string> GetSupportedArchiveEntryExtensions()
        {
            foreach (var name in Enum.GetNames(typeof(SupportedArchiveEntryExtensions)))
            {
                yield return name;
            }
        }

        public class CurrentStateEventArgs : EventArgs
        {
            public List<FileSystemGallery> FileSystemGalleries { get; set; } 
        }

        #endregion

        #region Fields

        private readonly ObservationType _observationType;
        private readonly DirectoryInfo _observedFolder;
        private readonly Timer _timer;
        private readonly object _lockObject = new object(); 

        #endregion

        #region Constructor

        public FolderObserver(DirectoryInfo observedFolder, 
                              ObservationType observationType = ObservationType.FoldersNonRecursive | ObservationType.FilesRecursive)
        {
            _observationType = observationType;
            _observedFolder = observedFolder;
            _timer = new Timer(TimerElapsed, null, 0, 5000);
        }

        private void TimerElapsed(object par)
        {
            if (Monitor.TryEnter(_lockObject))
            {
                try
                {
                    var state = UpdateCurrentState().ToList();
                    OnCurrentStateUpdated(state);
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }

        }

        #endregion Constructor

        #region Private methods

        private IEnumerable<FileSystemGallery> UpdateCurrentState()
        {
            IEnumerable<FileInfo> archives;
            IEnumerable<DirectoryInfo> archiveFolders;

            if (_observationType.HasFlag(ObservationType.FilesRecursive))
            {
                archives = GetFiles(_observedFolder, GetSupportedArchiveExtensions(), SearchOption.AllDirectories);
            }
            else if (_observationType.HasFlag(ObservationType.FilesNonRecursive))
            {
                archives = GetFiles(_observedFolder, GetSupportedArchiveExtensions(), SearchOption.TopDirectoryOnly);
            }
            else
            {
                archives = new List<FileInfo>();
            }

            if (_observationType.HasFlag(ObservationType.FoldersRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, GetSupportedArchiveEntryExtensions(), SearchOption.AllDirectories);
            }
            else if (_observationType.HasFlag(ObservationType.FoldersNonRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, GetSupportedArchiveEntryExtensions(), SearchOption.TopDirectoryOnly);
            }
            else
            {
                archiveFolders = new List<DirectoryInfo>();
            }

            foreach (var archiveFolder in archiveFolders)
            {
                yield return new FolderFileSystemGallery(archiveFolder);
            }
            foreach (var fileInfo in archives)
            {
                yield return new ArchiveFileSystemGallery(fileInfo);
            }
        }

        private IEnumerable<DirectoryInfo> GetArchiveDirectory(DirectoryInfo sourceFolder, IEnumerable<string> supportedImageExtensions, SearchOption searchOption)
        {
            var archiveDirectories = sourceFolder
                .GetDirectories("*", searchOption)
                .Where(x => !x.GetDirectories().Any())
                .Where(d => d.GetFiles().Any(f => supportedImageExtensions.Select(x => "." + x).Contains(f.Extension.ToLower())));

            return archiveDirectories;
        }

        private IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, 
            IEnumerable<string> supportedExtensions, 
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!supportedExtensions.Any())
            {
                return directoryInfo.GetFiles("*.*", searchOption);
            }
            else
            {
                return
                   supportedExtensions
                        .Select(x => "*." + x) // turn into globs
                        .SelectMany(x =>
                            directoryInfo.EnumerateFiles(x, searchOption)
                        );
            }
        }

        #endregion
        
        #region Events

        public event EventHandler<CurrentStateEventArgs> CurrentStateUpdated;

        protected virtual void OnCurrentStateUpdated(List<FileSystemGallery> param)
        {
            CurrentStateUpdated?.Invoke(this, new CurrentStateEventArgs {FileSystemGalleries = param});
        }

        #endregion
    }
}
