﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed view of a Node (e.g., an entry or group).
    /// </summary>
    /// <typeparam name="T">The type of Node.</typeparam>
    public interface INodeDetailsViewModel<T> : IDatabasePersistenceViewModel, IActiveDatabaseViewModel
        where T : IKeePassNode
    {
        /// <summary>
        /// Raised when the ViewModel requires a revert before updating IsReadOnly.
        /// </summary>
        event EventHandler RevertRequired;

        /// <summary>
        /// Whether or not editing is enabled for the View.
        /// </summary>
        bool IsReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not this child is new to the document (and so cannot be reverted).
        /// </summary>
        bool IsNew
        {
            get;
        }

        /// <summary>
        /// The Node being viewed in detail.
        /// </summary>
        T WorkingCopy
        {
            get;
        }

        /// <summary>
        /// Reverts any pending changes to the underlying document. This action can be cancelled.
        /// </summary>
        void Revert();

        /// <summary>
        /// Whether or not the WorkingCopy is different from the master.
        /// </summary>
        /// <returns></returns>
        bool IsDirty();
    }
}
