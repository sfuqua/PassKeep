// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// Abstracts synchronization of ViewModel activities with the UI thread.
    /// </summary>
    public interface ISyncContext
    {
        /// <summary>
        /// Whether the current thread is in sync with the thread represented by this context.
        /// </summary>
        bool IsSynchronized
        {
            get;
        }

        /// <summary>
        /// Runs the provided activity in this context.
        /// </summary>
        /// <param name="activity">The activity to invoke.</param>
        /// <returns>A <see cref="Task"/> representing the posted activity.</returns>
        Task Post(Action activity);
    }
}
