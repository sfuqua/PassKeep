// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// Helper extensions for dealing with MVVM functionality.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Asynchronously waits for a specific <see cref="INotifyPropertyChanged"/> event to fire.
        /// </summary>
        /// <param name="instance">The instance we are waiting on/extending.</param>
        /// <param name="evtName">The event to wait for - consider using nameof().</param>
        /// <returns></returns>
        public static async Task WaitForEventAsync(this INotifyPropertyChanged instance, string evtName)
        {
            TaskCompletionSource<bool> evtTcs = new TaskCompletionSource<bool>();
            PropertyChangedEventHandler handler = null;

            handler = (s, e) =>
            {
                if (e.PropertyName == evtName)
                {
                    evtTcs.SetResult(true);
                    instance.PropertyChanged -= handler;
                }
            };

            instance.PropertyChanged += handler;

            await evtTcs.Task;
        }

        /// <summary>
        /// Asynchronously waits for a specific <see cref="NotifyCollectionChangedAction"/> trigger to occur.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="ObservableCollection{T}"/> to wait on/extend.</typeparam>
        /// <param name="instance">The <see cref="ObservableCollection{T}"/> we are waiting on/extending.</param>
        /// <param name="change">The specific action to wait for.</param>
        /// <returns></returns>
        public static async Task WaitForChangeAsync<T>(this ObservableCollection<T> instance, NotifyCollectionChangedAction change)
        {
            TaskCompletionSource<bool> evtTcs = new TaskCompletionSource<bool>();
            NotifyCollectionChangedEventHandler handler = null;

            handler = (s, e) =>
            {
                if (e.Action == change)
                {
                    evtTcs.SetResult(true);
                    instance.CollectionChanged -= handler;
                }
            };

            instance.CollectionChanged += handler;

            await evtTcs.Task;
        }
    }
}
