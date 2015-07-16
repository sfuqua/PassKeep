using PassKeep.Lib.EventArgClasses;
using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep for contents of the root frame.
    /// </summary>
    public abstract class PassKeepPage : RootPassKeepPage
    {
        protected const string SavingResourceKey = "Saving";

        /// <summary>
        /// The thinnest possible view of an app ("snap" width in Windows 8).
        /// </summary>
        public const int SnapWidth = 320;

        /// <summary>
        /// Typical minimum width for an app.
        /// </summary>
        public const int NarrowWidth = 500;

        /// <summary>
        /// Bootstraps the NavigationHelper.
        /// </summary>
        protected PassKeepPage()
            : base()
        { }

        /// <summary>
        /// Invoked when the Page wishes to load a new KeePass database file.
        /// </summary>
        public event TypedEventHandler<PassKeepPage, StorageFile> FileLoadRequested;

        protected void RaiseFileLoadRequested(StorageFile file)
        {
            FileLoadRequested?.Invoke(this, file);
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has started a load
        /// operation.
        /// </summary>
        public event EventHandler<LoadingStartedEventArgs> StartedLoading;

        protected void RaiseStartedLoading(LoadingStartedEventArgs args)
        {
            StartedLoading?.Invoke(this, args);
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has finished
        /// a load operation.
        /// </summary>
        public event EventHandler DoneLoading;

        protected void RaiseDoneLoading()
        {
            DoneLoading?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Accesses the ContainerHelper for this page.
        /// </summary>
        public ContainerHelper ContainerHelper
        {
            get;
            internal set;
        }

        /// <summary>
        /// Handles the specified accelerator (Ctrl-modified) key.
        /// </summary>
        /// <param name="key">The hotkey to handle.</param>
        /// <returns>Whether this page can handle the provided hotkey.</returns>
        public virtual bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }
    }
}
