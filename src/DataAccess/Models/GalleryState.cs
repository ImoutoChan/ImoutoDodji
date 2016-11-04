namespace DataAccess.Models
{
    /// <summary>
    /// phases: 
    /// 1 Init
    /// 2 Search (and select)
    /// 3 Parsing
    /// 4 Saving
    /// </summary>
    public enum GalleryState
    {
        Init,
        SearchNotFound,
        SearchFoundAndSelected,
        SearchFoundAndWaitingForSelect,
        SearchSelected,
        SearchError,
        ParsingParsed,
        ParsingError,
        SavingSaved,
        SavingError
    }
}