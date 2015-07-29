using PassKeep.Lib.Contracts.Models;
using System.Windows.Input;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Interface for a ViewModel that represents a detailed, editable view of an IKeePassEntry.
    /// </summary>
    public interface IEntryDetailsViewModel : INodeDetailsViewModel<IKeePassEntry>
    {
        /// <summary>
        /// A ViewModel over the current field being edited.
        /// </summary>
        IFieldEditorViewModel FieldEditorViewModel
        {
            get;
        }

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

        /// <summary>
        /// Edits an existing field on the entry.
        /// </summary>
        ICommand EditFieldCommand
        {
            get;
        }

        /// <summary>
        /// Creates a new field for the entry and begins editing it.
        /// </summary>
        ICommand NewFieldCommand
        {
            get;
        }

        /// <summary>
        /// Commits the currently active field.
        /// </summary>
        ICommand CommitFieldCommand
        {
            get;
        }
    }
}
