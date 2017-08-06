// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using SariphLib.Mvvm;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of saved credentials, allowing
    /// them to be managed/deleted.
    /// </summary>
    public interface ISavedCredentialsViewModel : IViewModel
    {
        /// <summary>
        /// A command for deleting a specific credential.
        /// </summary>
        IAsyncCommand DeleteCredentialAsyncCommand { get; }

        /// <summary>
        /// A command for deleting all credentials.
        /// </summary>
        IAsyncCommand DeleteAllAsyncCommand { get; }

        /// <summary>
        /// A collection of credentials that are stored.
        /// </summary>
        ObservableCollection<string> CredentialTokens { get; }
    }
}
