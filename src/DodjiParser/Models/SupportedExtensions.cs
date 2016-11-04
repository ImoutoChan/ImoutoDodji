using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DodjiParser.Models
{
    public  enum SupportedArchiveExtensions
    {
        Zip,
        Rar,
        Cbz,
        Cbr
    }

    public enum SupportedImagesExtensions
    {
        Jpg,
        Png
    }



    public static class SupportedExtensions
    {

        public static IEnumerable<string> GetArchives()
        {
            return Enum.GetNames(typeof(SupportedArchiveExtensions)).Select(name => name.ToLower());
        }

        public static IEnumerable<string> GetImages()
        {
            return Enum.GetNames(typeof(SupportedImagesExtensions)).Select(name => name.ToLower());
        }

        public static IEnumerable<FileInfo> GetFilesWithExtensions(DirectoryInfo directoryInfo,
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
