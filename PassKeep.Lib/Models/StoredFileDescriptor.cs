using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage.AccessCache;

namespace PassKeep.Models
{
    /// <summary>
    /// Class that proxies AccessListEntry to allow databinding.
    /// </summary>
    public class StoredFileDescriptor : BindableBase
    {
        private readonly AsyncActionCommand forgetCommand;
        private readonly ActionCommand exportCommand;
        private readonly AsyncActionCommand updateCommand;
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

            this.forgetCommand = new AsyncActionCommand(FireForgetRequested);
            this.exportCommand = new ActionCommand(FireExportRequested);
            this.updateCommand = new AsyncActionCommand(() => IsAppOwned, FireUpdateRequested);
            this.openCommand = new ActionCommand(FireOpenRequested);
        }

        /// <summary>
        /// Fired when a user requests to forget/delete a stored file.
        /// </summary>
        public event TypedEventHandler<StoredFileDescriptor, RequestForgetDescriptorEventArgs> ForgetRequested;

        /// <summary>
        /// Fired when the user requests to export a stored file to another location.
        /// </summary>
        public event TypedEventHandler<StoredFileDescriptor, EventArgs> ExportRequested;

        /// <summary>
        /// Fired when the user requests to update the file that backs a StoredFileDescriptor.
        /// </summary>
        public event TypedEventHandler<StoredFileDescriptor, RequestUpdateDescriptorEventArgs> UpdateRequested;

        /// <summary>
        /// Fired when the user requests to open a stored file.
        /// </summary>
        public event TypedEventHandler<StoredFileDescriptor, EventArgs> OpenRequested;

        /// <summary>
        /// Gets the command used to export this file.
        /// </summary>
        public ICommand ExportCommand
        {
            get { return this.exportCommand; }
        }

        /// <summary>
        /// Gets the command used to update this file.
        /// </summary>
        public IAsyncCommand UpdateCommand
        {
            get { return this.updateCommand; }
        }

        /// <summary>
        /// Gets the command used to open this file.
        /// </summary>
        public ICommand OpenCommand
        {
            get { return this.openCommand; }
        }

        /// <summary>
        /// Gets the command used to forget this file.
        /// </summary>
        public IAsyncCommand ForgetCommand
        {
            get { return this.forgetCommand; }
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
                    this.updateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Private helper to fire <see cref="ForgetRequested"/>.
        /// </summary>
        private Task FireForgetRequested()
        {
            RequestForgetDescriptorEventArgs args = new RequestForgetDescriptorEventArgs(this);
            ForgetRequested?.Invoke(this, args);
            return args.DeferAsync();
        }

        /// <summary>
        /// Private helper to fire <see cref="ExportRequested"/>.
        /// </summary>
        private void FireExportRequested()
        {
            ExportRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Private helper to fire <see cref="UpdateRequested"/>.
        /// </summary>
        private Task FireUpdateRequested()
        {
            RequestUpdateDescriptorEventArgs args = new RequestUpdateDescriptorEventArgs(this);
            UpdateRequested?.Invoke(this, args);
            return args.DeferAsync();
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
