// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        /// The backing string for <see cref="WorkingCopy"/>, or null for a new field.
        /// </summary>
        IProtectedString Original { get; }

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
