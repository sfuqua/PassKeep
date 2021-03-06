﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Providers;
using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep for contents of the root frame.
    /// </summary>
    public abstract class PassKeepPage : BasePassKeepPage
    {
        protected const string SavingResourceKey = "Saving";

        /// <summary>
        /// The thinnest possible view of an app ("snap" width in Windows 8).
        /// </summary>
        public const int SnapWidth = 320;

        /// <summary>
        /// Typical minimum width for an app.
        /// </summary>
        public const int NarrowWidth = 500;

        /// <summary>
        /// Bootstraps the page.
        /// </summary>
        protected PassKeepPage()
            : base()
        { }

        /// <summary>
        /// Invoked when the Page wishes to load a new KeePass database file.
        /// </summary>
        public event TypedEventHandler<PassKeepPage, StorageFile> FileLoadRequested;

        protected void RaiseFileLoadRequested(StorageFile file)
        {
            FileLoadRequested?.Invoke(this, file);
        }

        /// <summary>
        /// Used to assemble database candidates when the user opens a file.
        /// </summary>
        public IDatabaseCandidateFactory DatabaseCandidateFactory
        {
            protected get;
            set;
        }

        /// <summary>
        /// Handles the specified accelerator (Ctrl-modified) key.
        /// </summary>
        /// <param name="key">The hotkey to handle.</param>
        /// <param name="shift">Whether shift is also depressed.</param>
        /// <returns>Whether this page can handle the provided hotkey.</returns>
        public virtual bool HandleAcceleratorKey(VirtualKey key, bool shift)
        {
            return false;
        }

        /// <summary>
        /// Saves state when app is being suspended.
        /// </summary>
        public virtual void HandleSuspend() { }

        /// <summary>
        /// Restores state when app is resumed.
        /// </summary>
        public virtual void HandleResume() { }
    }
}
