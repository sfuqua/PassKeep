using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// Represents a CollectionView over a collection of objects, with sorting capabilities.
    /// </summary>
    public class SortableCollectionView : BindableBase, ICollectionViewEx
    {
        private int index;
        private List<object> internalView = new List<object>();

        /// <summary>
        /// Grouping is not supported by this CollectionView.
        /// </summary>
        public IObservableVector<object> CollectionGroups
        {
            get { return null; }
        }

        /// <summary>
        /// Fired after the current item has been changed.
        /// </summary>
        public event EventHandler<object> CurrentChanged;
        private void RaiseCurrentChanged()
        {
            if (CurrentChanged != null)
            {
                CurrentChanged(this, CurrentItem);
            }
        }

        /// <summary>
        /// Fired before changing the current item. The event handler can cancel this event.
        /// </summary>
        public event CurrentChangingEventHandler CurrentChanging;
        private CurrentChangingEventArgs RaiseCurrentChanging(bool cancelable)
        {
            CurrentChangingEventArgs args = new CurrentChangingEventArgs(cancelable);
            if (CurrentChanging != null)
            {
                CurrentChanging(this, args);
            }

            return args;
        }

        /// <summary>
        /// The current item in the view, or null if there is no current item.
        /// </summary>
        public object CurrentItem
        {
            get
            {
                if (this.IsCurrentBeforeFirst || this.IsCurrentAfterLast)
                {
                    return null;
                }

                return this.internalView[this.index];
            }
        }

        /// <summary>
        /// The oridinal position of the CurrentItem within the view.
        /// </summary>
        public int CurrentPosition
        {
            get { return this.index; }
        }

        /// <summary>
        /// LoadMoreItemsAsync is not supported.
        /// </summary>
        public bool HasMoreItems
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value that indicates whether the CurrentItem of the view is beyond the end 
        /// of the collection.
        /// </summary>
        public bool IsCurrentAfterLast
        {
            get { return this.index >= this.internalView.Count; }
        }

        /// <summary>
        /// Gets a value that indicates whether the CurrentItem of the view is beyond the beginning of the collection.
        /// </summary>
        public bool IsCurrentBeforeFirst
        {
            get { return this.index < 0; }
        }

        /// <summary>
        /// Not supported - initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">The number of items to load.</param>
        /// <returns>The wrapped results of the load operation.</returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the specified item to be the CurrentItem in the view.
        /// </summary>
        /// <param name="item">The item to set as the CurrentItem.</param>
        /// <returns>True if the resulting CurrentItem is within the view; otherwise, false.</returns>
        public bool MoveCurrentTo(object item)
        {
            if (item == this.CurrentItem)
            {
                return true;
            }
            
            return MoveCurrentToPosition(IndexOf(item));
        }

        /// <summary>
        /// Sets the first item in the view as the CurrentItem.
        /// </summary>
        /// <returns>True is the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public bool MoveCurrentToFirst()
        {
            return MoveCurrentToPosition(0);
        }

        /// <summary>
        /// Sets the last item in the view as the CurrentItem.
        /// </summary>
        /// <returns>True if the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public bool MoveCurrentToLast()
        {
            return MoveCurrentToPosition(this.internalView.Count - 1);
        }

        /// <summary>
        /// Sets the item after the CurrentItem in the view as the CurrentItem.
        /// </summary>
        /// <returns>True if the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public bool MoveCurrentToNext()
        {
            return MoveCurrentToPosition(this.index + 1);
        }

        /// <summary>
        /// Sets the item at the specified index to be the CurrentItem in the view.
        /// </summary>
        /// <param name="index">The index of the item to move to.</param>
        /// <returns>True if the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public bool MoveCurrentToPosition(int index)
        {
            if (this.index == index)
            {
                return !this.IsCurrentBeforeFirst && !this.IsCurrentAfterLast;
            }

            CurrentChangingEventArgs eventArgs = RaiseCurrentChanging(true);
            if (eventArgs.Cancel)
            {
                return false;
            }

            this.index = index;
            RaiseCurrentChanged();

            return !this.IsCurrentBeforeFirst && !this.IsCurrentAfterLast;
        }

        /// <summary>
        /// Sets the item before the CurrentItem in the view as the CurrentItem.
        /// </summary>
        /// <returns>True if the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public bool MoveCurrentToPrevious()
        {
            return MoveCurrentToPosition(this.index - 1);
        }

        /// <summary>
        /// Fired when the vector changes.
        /// </summary>
        public event VectorChangedEventHandler<object> VectorChanged;
        private void RaiseVectorChanged(CollectionChange change, uint index)
        {
            if (VectorChanged != null)
            {
                VectorChanged(this, new VectorChangedEventArgs(change, index));
            }
        }

        #region IList implementation

        public int IndexOf(object item)
        {
            return this.internalView.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            this.internalView.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.internalView.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return this.internalView[index];
            }
            set
            {
                this.internalView[index] = value;
            }
        }

        public void Add(object item)
        {
            this.internalView.Add(item);
        }

        public void Clear()
        {
            this.internalView.Clear();
        }

        public bool Contains(object item)
        {
            return this.internalView.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            this.internalView.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.internalView.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList)this.internalView).IsReadOnly; }
        }

        public bool Remove(object item)
        {
            return this.internalView.Remove(item);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return this.internalView.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.internalView.GetEnumerator();
        }

        #endregion
    }
}
