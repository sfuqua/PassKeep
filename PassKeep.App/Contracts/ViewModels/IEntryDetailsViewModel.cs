using PassKeep.Lib.Contracts.Models;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Interface for a ViewModel that represents a detailed, editable view of an IKeePassEntry.
    /// </summary>
    public interface IEntryDetailsViewModel : INodeDetailsViewModel<IKeePassEntry>
    {
        /// <summary>
        /// Copies the value of a field to the clipboard.
        /// </summary>
        ICommand CopyFieldValueCommand
        {
            get;
        }

        /// <summary>
        /// Deletes a field from the entry.
        /// </summary>
        ICommand DeleteFieldCommand
        {
            get;
        }
    }
}
