using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Contracts.Models;
using System;
using System.Windows.Input;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that wraps an <see cref="IKeePassNode"/>.
    /// </summary>
    public class DatabaseNodeViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Initializes the proxy.
        /// </summary>
        /// <param name="node"></param>
        public DatabaseNodeViewModel(IKeePassNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            this.Node = node;
            this.RequestRenameCommand = new ActionCommand(FireRenameRequested);
            this.RequestEditDetailsCommand = new ActionCommand(FireEditRequested);
            this.RequestDeleteCommand = new ActionCommand(FireDeleteRequested);
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
