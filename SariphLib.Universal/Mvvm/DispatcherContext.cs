// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An <see cref="ISyncContext"/> that is represented by a <see cref="CoreDispatcher"/>.
    /// </summary>
    public sealed class DispatcherContext : ISyncContext
    {
        CoreDispatcher dispatcher;

        /// <summary>
        /// Initializes the context with the main view's <see cref="CoreWindow"/> dispatcher.
        /// </summary>
        public DispatcherContext()
            : this(CoreApplication.MainView.CoreWindow.Dispatcher)
        { }

        /// <summary>
        /// Initializes the context with the specified dispatcher.
        /// </summary>
        /// <param name="dispatcher"></param>
        public DispatcherContext(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Whether the current thread is the UI thread (as determined by the dispatcher).
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return this.dispatcher.HasThreadAccess;
            }
        }

        /// <summary>
        /// Dispatches <paramref name="activity"/> at normal priority.
        /// </summary>
        /// <param name="activity">The activity to dispatch.</param>
        /// <returns>A <see cref="Task"/> representing the dispatch.</returns>
        public async Task Post(Action activity)
        {
            if (IsSynchronized)
            {
                activity();
                await Task.FromResult<int>(0);
            }
            else
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => activity());
            }
        }
    }
}
