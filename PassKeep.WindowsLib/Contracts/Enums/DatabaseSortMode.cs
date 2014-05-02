using SariphLib.Mvvm;

namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Represents different available methods for sorting the document view.
    /// </summary>
    public class DatabaseSortMode : BindableBase
    {
        private Mode _sortMode;
        private string _friendlyName;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="mode">The actual sort mode to represent.</param>
        /// <param name="name">A friendly name for the sorting method.</param>
        public DatabaseSortMode(Mode mode, string name)
        {
            this._sortMode = mode;
            this._friendlyName = name;
        }

        /// <summary>
        /// The Mode represented by this instance.
        /// </summary>
        public Mode SortMode
        {
            get { return this._sortMode; }
            set { TrySetProperty(ref this._sortMode, value); }
        }

        /// <summary>
        /// Returns a friendly name for this sort mode instance.
        /// </summary>
        /// <returns>A friendly name for the sort method.</returns>
        public override string ToString()
        {
            return this._friendlyName;
        }

        /// <summary>
        /// The actual mode used for sorting.
        /// </summary>
        public enum Mode : int
        {
            /// <summary>
            /// Sort by the default order provided by the document file.
            /// </summary>
            DatabaseOrder = 0,

            /// <summary>
            /// Sort in A-Z order.
            /// </summary>
            AlphabetAscending,

            /// <summary>
            /// Sort in Z-A order.
            /// </summary>
            AlphabetDescending
        }
    }
}
