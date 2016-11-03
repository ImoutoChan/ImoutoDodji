using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;

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
    /// *FoldersRecursive* - not supported yet
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
        public struct FileSystemEntry
        {
            public bool IsFolder { get; set; }

            public string Path { get; set; }
        }

        private enum SupportedArchiveExtensions
        {
            Zip,
            Rar
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

        [Flags]
        public enum ObservationType
        {
            FilesNonRecursive = 1,
            FoldersNonRecursive = 2,
            FilesRecursive = 5
        }

        private readonly ObservationType _observationType;
        private readonly DirectoryInfo _observedFolder;

        public FolderObserver(DirectoryInfo observedFolder, 
                              ObservationType observationType = ObservationType.FoldersNonRecursive | ObservationType.FilesRecursive,
                              IEnumerable<FileSystemEntry> ignoreEntries = null
                              )
        {
            _observationType = observationType;
            _observedFolder = observedFolder;

            Update();
        }

        private void Update()
        {
            IEnumerable<FileInfo> archives;
            IEnumerable<DirectoryInfo> archiveFolders;

            if (_observationType.HasFlag(ObservationType.FilesRecursive))
            {
                archives = GetFiles(_observedFolder, GetSupportedArchiveExtensions(), SearchOption.AllDirectories);
            }
            else if (_observationType.HasFlag(ObservationType.FilesNonRecursive))
            {
                archives = GetFiles(_observedFolder, GetSupportedArchiveExtensions(), SearchOption.AllDirectories);
            }
            else
            {
                archives = new List<FileInfo>();
            }

            if (_observationType.HasFlag(ObservationType.FoldersNonRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, GetSupportedArchiveEntryExtensions(), SearchOption.TopDirectoryOnly);
            }
            else if (_observationType.HasFlag(ObservationType.FilesNonRecursive))
            {
                archiveFolders = GetArchiveDirectory(_observedFolder, GetSupportedArchiveEntryExtensions(), SearchOption.AllDirectories);
            }
            else
            {
                archiveFolders = new List<DirectoryInfo>();
            }
        }

        private IEnumerable<DirectoryInfo> GetArchiveDirectory(DirectoryInfo sourceFolder, IEnumerable<string> supportedExtensions, SearchOption searchOption)
        {
            var archiveDirectories = sourceFolder
                .GetDirectories(String.Empty, searchOption)
                .Where(x => !x.GetDirectories().Any())
                .Where(d => d.GetFiles().Any(f => supportedExtensions.Contains(f.Extension.ToLower())));

            foreach (var directoryInfo in archiveDirectories)
            {
                
            }
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
    }
}
