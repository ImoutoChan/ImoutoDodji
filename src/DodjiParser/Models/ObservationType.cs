using System;
namespace DodjiParser.Models
{
    [Flags]
    public enum ObservationType
    {
        FilesNonRecursive = 1,
        FoldersNonRecursive = 2,
        FilesRecursive = 5, // include FilesNonRecursive
        FoldersRecursive = 10, // include FoldersNonRecursive
        All = 15
    }
}
