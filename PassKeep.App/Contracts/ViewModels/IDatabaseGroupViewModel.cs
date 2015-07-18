using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Extends <see cref="IDatabaseNodeViewModel"/> to enable
    /// group specific tasks.
    /// </summary>
    public interface IDatabaseGroupViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Fired when the group is requested to be opened.
        /// </summary>
        event EventHandler OpenRequested;

        /// <summary>
        /// Command for requesting to open the current group.
        /// </summary>
        ICommand RequestOpenCommand { get; }
    }
}
