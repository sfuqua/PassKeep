﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// ViewModel for a view over an unlocked database, regardless of the details of the view.
    /// </summary>
    public interface IDatabaseParentViewModel : IDatabasePersistenceViewModel, IActiveDatabaseViewModel
    {
        /// <summary>
        /// Fired when the View should lock the current workspace.
        /// </summary>
        event EventHandler LockRequested;

        /// <summary>
        /// The file on disk represented by this database.
        /// </summary>
        ITestableFile File
        {
            get;
        }

        /// <summary>
        /// Whether <see cref="File"/> is a sample database.
        /// </summary>
        bool FileIsSample
        {
            get;
        }

        /// <summary>
        /// Generates an <see cref="IDatabaseViewModel"/> based on current state.
        /// </summary>
        /// <returns>A ViewModel over the database tree.</returns>
        IDatabaseViewModel GetDatabaseViewModel();

        /// <summary>
        /// Attempts to lock the workspace manually.
        /// </summary>
        void TryLock();

        /// <summary>
        /// Notifies the ViewModel of user interactivity to reset the idle timer.
        /// </summary>
        void HandleInteractivity();
    }
}
