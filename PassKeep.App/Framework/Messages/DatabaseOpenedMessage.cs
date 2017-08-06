// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using Windows.Storage;

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// A message for communicating that a new database file was opened.
    /// </summary>
    public sealed class DatabaseOpenedMessage : MessageBase
    {
        /// <summary>
        /// Constructs the message.
        /// </summary>
        /// <param name="viewModel">A ViewModel representing the opened database.</param>
        public DatabaseOpenedMessage(IDatabaseParentViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        /// <summary>
        /// A ViewModel for the opened database.
        /// </summary>
        public IDatabaseParentViewModel ViewModel
        {
            get;
            private set;
        }
    }
}
