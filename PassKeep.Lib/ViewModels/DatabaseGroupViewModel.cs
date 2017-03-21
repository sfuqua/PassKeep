using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Extends <see cref="DatabaseNodeViewModel"/> to wrap a group, specifically.
    /// </summary>
    public sealed class DatabaseGroupViewModel : DatabaseNodeViewModel, IDatabaseGroupViewModel
    {
        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="group">The database group to proxy.</param>
        /// <param name="isReadOnly">Whether the database is in a state that can be edited.</param>
        public DatabaseGroupViewModel(IKeePassGroup group, bool isReadOnly)
            : base(group, isReadOnly)
        {
            RequestOpenCommand = new ActionCommand(FireOpenRequested);
        }

        /// <summary>
        /// Fired when the group is requested to be opened.
        /// </summary>
        public event EventHandler OpenRequested;

        private void FireOpenRequested()
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Command for requesting to open the current group.
        /// </summary>
        public ICommand RequestOpenCommand
        {
            get;
            private set;
        }
    }
}
