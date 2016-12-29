using SariphLib.Mvvm;
using System;
using System.Windows.Input;
using Windows.Storage.AccessCache;

namespace PassKeep.Models
{
    /// <summary>
    /// Class that proxies AccessListEntry to allow databinding.
    /// </summary>
    public class StoredFileDescriptor : BindableBase
    {
        private readonly ActionCommand exportCommand;
        private readonly ActionCommand openCommand;
        private readonly AccessListEntry accessListEntry;

        private bool isAppOwned;

        /// <summary>
        /// Initializes the model from the provided struct.
        /// </summary>
        /// <param name="accessListEntry">The AccessListEntry to use as a reference.</param>
        public StoredFileDescriptor(AccessListEntry accessListEntry)
        {
            this.accessListEntry = accessListEntry;
            this.isAppOwned = false;

            this.exportCommand = new ActionCommand(() => IsAppOwned, FireExportRequested);
            this.openCommand = new ActionCommand(FireOpenRequested);
        }

        /// <summary>
        /// Fired when the user requests to export a stored file to another location.
        /// </summary>
        public event EventHandler ExportRequested;

        /// <summary>
        /// Fired when the user requests to open a stored file.
        /// </summary>
        public event EventHandler OpenRequested;

        /// <summary>
        /// Gets the command used to export this file.
        /// </summary>
        public ICommand ExportCommand
        {
            get { return this.exportCommand; }
        }

        /// <summary>
        /// Gets the command used to open this file.
        /// </summary>
        public ICommand OpenCommand
        {
            get { return this.openCommand; }
        }

        /// <summary>
        /// Returns an access token for the stored file.
        /// </summary>
        public string Token
        {
            get
            {
                return this.accessListEntry.Token;
            }
        }

        /// <summary>
        /// Returns metadata about the stored file.
        /// </summary>
        public string Metadata
        {
            get
            {
                return this.accessListEntry.Metadata;
            }
        }

        /// <summary>
        /// Whether PassKeep controls this file.
        /// </summary>
        public bool IsAppOwned
        {
            get
            {
                return this.isAppOwned;
            }
            set
            {
                if (TrySetProperty(ref this.isAppOwned, value))
                {
                    this.exportCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Private helper to fire <see cref="ExportRequested"/>.
        /// </summary>
        private void FireExportRequested()
        {
            ExportRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Private helper to fire <see cref="OpenRequested"/>.
        /// </summary>
        private void FireOpenRequested()
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
