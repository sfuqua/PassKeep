using SariphLib.Eventing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// A readonly observable collection that represents a transform/projection of an existing observable collection.
    /// </summary>
    /// <typeparam name="TSource">The type to project from.</typeparam>
    /// <typeparam name="TProject">The type to project to.</typeparam>
    public class ReadOnlyObservableCollectionTransform<TSource, TProject>: BindableBase, INotifyCollectionChanged, IReadOnlyCollection<TProject>
    {
        private List<TProject> projection;
        private readonly Func<TSource, TProject> transformer;

        /// <summary>
        /// Initializes this collection from a source collection.
        /// </summary>
        /// <param name="source">The collection to wrap.</param>
        /// <param name="transformer">A function that performs the projection/transformation.</param>
        public ReadOnlyObservableCollectionTransform(ObservableCollection<TSource> source, Func<TSource, TProject> transformer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            source.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(HandleCollectionChanged).Handler;

            BuildProjection(source);
        }

        /// <summary>
        /// Invoked on behalf of the wrapped collection whenever it changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var source = (ObservableCollection<TSource>)sender;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var added = new List<TProject>(args.NewItems.Count);
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        added.Add(this.transformer((TSource)args.NewItems[i]));
                    }

                    this.projection.InsertRange(args.NewStartingIndex, added);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, added, args.NewStartingIndex));
                    OnPropertyChanged(nameof(Count));

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (args.NewItems.Count > 1)
                    {
                        throw new NotImplementedException();
                    }

                    TProject item = this.projection[args.OldStartingIndex];
                    this.projection.RemoveAt(args.OldStartingIndex);
                    this.projection.Insert(args.NewStartingIndex, item);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, item, args.OldStartingIndex, args.NewStartingIndex));

                    break;
                case NotifyCollectionChangedAction.Remove:
                    var removed = new List<TProject>(args.OldItems.Count);
                    for (int i = 0; i < args.OldItems.Count; i++)
                    {
                        removed.Add(this.projection[i + args.OldStartingIndex]);
                    }

                    this.projection.RemoveRange(args.OldStartingIndex, args.OldItems.Count);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, removed, args.OldStartingIndex));
                    OnPropertyChanged(nameof(Count));

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (args.NewItems.Count != args.OldItems.Count || args.NewStartingIndex != args.OldStartingIndex)
                    {
                        throw new InvalidOperationException();
                    }

                    var newItems = new List<TProject>(args.NewItems.Count);
                    var replaced = new List<TProject>(args.NewItems.Count);

                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        TProject newItem = this.transformer((TSource)args.NewItems[i]);
                        replaced.Add(this.projection[i + args.NewStartingIndex]);
                        newItems.Add(newItem);
                        this.projection[i + args.NewStartingIndex] = newItem;
                    }

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, newItems, replaced, args.NewStartingIndex));

                    break;
                case NotifyCollectionChangedAction.Reset:
                    BuildProjection(source);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void BuildProjection(ObservableCollection<TSource> source)
        {
            this.projection = new List<TProject>(source.Count);
            foreach (TSource sourceEle in source)
            {
                this.projection.Add(this.transformer(sourceEle));
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
        }

        #region IReadOnlyCollection

        public int Count => this.projection.Count;

        public IEnumerator<TProject> GetEnumerator() => this.projection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
