using System.Collections.ObjectModel;
using DataAccess.Models;

namespace DataAccess
{
    internal class DataCache
    {
        public bool IsCollectionsInitialized { get; set; } = false;
        public ObservableCollection<Collection> Collections { get; set; } = new ObservableCollection<Collection>();

        //public ObservableCollection<SourceFolder> SourceFolders { get; set; } = new ObservableCollection<SourceFolder>();

        //public ObservableCollection<DestinationFolder> DestinationFolders { get; set; } = new ObservableCollection<DestinationFolder>();

        public bool IsGalleriesInitialized { get; set; } = false;
        public ObservableCollection<Gallery> Galleries { get; set; } = new ObservableCollection<Gallery>();
    }
}