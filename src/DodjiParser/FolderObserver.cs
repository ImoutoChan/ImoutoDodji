using DodjiParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SharedModel;

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
    public class FolderObserver : IDisposable
    {
        #region Types

        public class CurrentStateEventArgs : EventArgs
        {
            public List<IFileSystemGallery> FileSystemGalleries { get; set; } 
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
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }

        }

        #endregion Constructor
        
        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #region Private methods

        private IEnumerable<IFileSystemGallery> UpdateCurrentState()
        {
            IEnumerable<FileInfo> archives;
            IEnumerable<DirectoryInfo> archiveFolders;

            if (_observationType.HasFlag(ObservationType.FilesRecursive))
            {
                archives = SupportedExtensions.GetFilesWithExtensions(_observedFolder, SupportedExtensions.GetArchives(), SearchOption.AllDirectories);
            }
            else if (_observationType.HasFlag(ObservationType.FilesNonRecursive))
            {
                archives = SupportedExtensions.GetFilesWithExtensions(_observedFolder, SupportedExtensions.GetArchives());
            }
            else
            {
                archives = new List<FileInfo>();
            }

            if (_observationType.HasFlag(ObservationType.FoldersRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, SupportedExtensions.GetImages(), SearchOption.AllDirectories);
            }
            else if (_observationType.HasFlag(ObservationType.FoldersNonRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, SupportedExtensions.GetImages());
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

        private IEnumerable<DirectoryInfo> GetArchiveDirectory(DirectoryInfo sourceFolder, IEnumerable<string> supportedImageExtensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var archiveDirectories = sourceFolder
                .GetDirectories("*", searchOption)
                .Where(x => !x.GetDirectories().Any())
                .Where(d => d.GetFiles().Any(f => supportedImageExtensions.Select(x => "." + x).Contains(f.Extension.ToLower())));

            return archiveDirectories;
        }
        
        #endregion
        
        #region Events

        public event EventHandler<CurrentStateEventArgs> CurrentStateUpdated;

        protected virtual void OnCurrentStateUpdated(List<IFileSystemGallery> param)
        {
            CurrentStateUpdated?.Invoke(this, new CurrentStateEventArgs {FileSystemGalleries = param});
        }

        #endregion
    }
}
