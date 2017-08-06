// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Eventing;
using System.Threading.Tasks;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Handles asking the user for permission to do some action.
    /// </summary>
    public class ConsentRequestedEventArgs : DeferrableEventArgs
    {
        private readonly object syncRoot = new object();
        private readonly bool defaultValue;

        private int approvals;
        private int rejections;

        /// <summary>
        /// Initializes the event args with the default value.
        /// </summary>
        /// <param name="defaultValue">The consent state if no subscribers act on the event.</param>
        public ConsentRequestedEventArgs(bool defaultValue = true)
        {
            this.defaultValue = defaultValue;
            this.approvals = 0;
            this.rejections = 0;
        }

        /// <summary>
        /// Whether consent has been granted.
        /// </summary>
        public bool ConsentGranted
        {
            get
            {
                bool rejected = (this.rejections > 0);
                if (this.defaultValue)
                {
                    // If the default is true, one rejection wins
                    return !rejected;
                }
                else
                {
                    // If the default is false, we require at least one approval
                    return this.approvals > 0 && !rejected;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to defer and then check the state of <see cref="ConsentGranted"/>.
        /// </summary>
        /// <returns>Whether consent is granted after all outstanding deferrals are released.</returns>
        public async Task<bool> GetConsentAsync()
        {
            await DeferAsync();
            return ConsentGranted;
        }

        /// <summary>
        /// Indicates a vote for granting consent.
        /// </summary>
        public void Approve()
        {
            lock (this.syncRoot)
            {
                this.approvals++;
            }
        }

        /// <summary>
        /// Indicates a vote for refusing consent.
        /// </summary>
        public void Reject()
        {
            lock (this.syncRoot)
            {
                this.rejections++;
            }
        }
    }
}
