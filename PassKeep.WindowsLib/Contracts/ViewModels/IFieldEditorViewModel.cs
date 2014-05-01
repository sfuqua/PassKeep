using PassKeep.Lib.Contracts.Models;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents an interface for a ViewModel used to edit custom fields of an entry.
    /// </summary>
    public interface IFieldEditorViewModel : IViewModel
    {
        /// <summary>
        /// Gets the copy of the string that is currently being manipulated.
        /// </summary>
        IProtectedString WorkingCopy { get; }

        /// <summary>
        /// Gets the command used for persisting changes to the string.
        /// </summary>
        ICommand CommitCommand { get; }

        /// <summary>
        /// Gets the last validation failure message, if one exists.
        /// </summary>
        string ValidationError { get; }
    }
}
