using SariphLib.Mvvm;

namespace PassKeep.Lib.Models
{
    public class DatabaseSortMode : BindableBase
    {
        public enum Mode : int
        {
            DatabaseOrder = 0,
            AlphabetAscending,
            AlphabetDescending
        }

        private Mode _sortMode;
        public Mode SortMode
        {
            get { return _sortMode; }
            set { TrySetProperty(ref _sortMode, value); }
        }

        private string _friendlyName;
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { TrySetProperty(ref _friendlyName, value); }
        }

        public DatabaseSortMode(Mode mode, string name)
        {
            SortMode = mode;
            FriendlyName = name;
        }
    }
}
