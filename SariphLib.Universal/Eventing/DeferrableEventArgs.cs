// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SariphLib.Eventing
{
    /// <summary>
    /// A basic EventArgs object that allows event listeners to 
    /// get a <see cref="Deferral"/> that will be awaited before
    /// the event is considered handled.
    /// </summary>
    public class DeferrableEventArgs : EventArgs
    {
        private ICollection<TaskCompletionSource<bool>> deferralCompletions;

        public DeferrableEventArgs()
        {
            this.deferralCompletions = new List<TaskCompletionSource<bool>>();
        }

        /// <summary>
        /// Adds an outstanding deferral to the internal deferral tracker such that 
        /// <see cref="DeferAsync"/> will not complete until this deferral does.
        /// </summary>
        /// <returns></returns>
        public Deferral GetDeferral()
        {
            TaskCompletionSource<bool> deferralCompletion = new TaskCompletionSource<bool>();
            Deferral deferral = new Deferral(() => deferralCompletion.SetResult(true));
            this.deferralCompletions.Add(deferralCompletion);
            return deferral;
        }

        /// <summary>
        /// Returns a task that completes when all outstanding deferrals are completed.
        /// </summary>
        /// <returns></returns>
        public Task DeferAsync()
        {
            return Task.WhenAll(this.deferralCompletions.Select(tcs => tcs.Task));
        }
    }
}
