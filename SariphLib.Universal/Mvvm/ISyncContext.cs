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
