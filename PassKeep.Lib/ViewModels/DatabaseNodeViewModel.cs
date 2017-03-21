using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Contracts.Models;
using System;
using System.Windows.Input;
using SariphLib.Mvvm;
using PassKeep.Lib.Contracts.Services;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that wraps an <see cref="IKeePassNode"/>.
    /// </summary>
    public class DatabaseNodeViewModel : IDatabaseNodeViewModel
    {
        private Func<bool> canEdit;

        /// <summary>
        /// Initializes the proxy.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="readOnly">Whether the database is in a state that can be edited.</param>
        public DatabaseNodeViewModel(IKeePassNode node, bool readOnly = false)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Node = node;

            this.canEdit = () => !readOnly;

            RequestRenameCommand = new ActionCommand(canEdit, FireRenameRequested);
            RequestEditDetailsCommand = new ActionCommand(canEdit, FireEditRequested);
            RequestDeleteCommand = new ActionCommand(canEdit, FireDeleteRequested);
        }

        /// <summary>
        /// Fired when the user requests a rename of this node.
        /// </summary>
        public event EventHandler RenameRequested;

        private void FireRenameRequested()
        {
            RenameRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when the user requests to edit the details of this node.
        /// </summary>
        public event EventHandler EditRequested;

        private void FireEditRequested()
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when the user requests to delete this node.
        /// </summary>
        public event EventHandler DeleteRequested;

        private void FireDeleteRequested()
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Provides access to the wrapped node.
        /// </summary>
        public IKeePassNode Node
        {
            get;
            private set;
        }

        /// <summary>
        /// Command for requesting a rename prompt for the current node.
        /// </summary>
        public ICommand RequestRenameCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Command for requesting to edit the details for the current node.
        /// </summary>
        public ICommand RequestEditDetailsCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Command for requesting deletion of the current node.
        /// </summary>
        public ICommand RequestDeleteCommand
        {
            get;
            private set;
        }
    }
}
