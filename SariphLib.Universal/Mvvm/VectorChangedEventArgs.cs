using Windows.Foundation.Collections;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// Implementation of IVectorChangedEventArgs, useful for firing events.
    /// </summary>
    public class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public VectorChangedEventArgs(CollectionChange change, uint index)
        {
            this.CollectionChange = change;
            this.Index = index;
        }

        /// <summary>
        /// The type of the change that occurred in the vector.
        /// </summary>
        public CollectionChange CollectionChange
        {
            get;
            private set;
        }

        /// <summary>
        /// The zero-based position where the change occurred in the vector.
        /// </summary>
        public uint Index
        {
            get;
            private set;
        }
    }
}
