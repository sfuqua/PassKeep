// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System.Threading.Tasks;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// A wrapper around Task that is data-binding friendly.
    /// </summary>
    /// <remarks>
    /// Adapted from <see cref="https://msdn.microsoft.com/magazine/dn605875"/>.
    /// Thanks to Stephen Cleary.
    /// </remarks>
    /// <typeparam name="TResult">The result type of the wrapped task.</typeparam>
    public sealed class NotifyTaskCompletion<TResult> : NotifyTaskCompletion
    {
        /// <summary>
        /// Wraps the specified <see cref="Task"/> in a databindable object.
        /// </summary>
        /// <param name="task">The Task to wrap.</param>
        public NotifyTaskCompletion(Task<TResult> task)
            : base(task)
        {
            Task = task;
        }

        /// <summary>
        /// The wrapped task.
        /// </summary>
        public new Task<TResult> Task
        {
            get;
            private set;
        }

        #region Wrapped properties

        public TResult Result
        {
            get
            {
                return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
            }
        }

        #endregion

        /// <summary>
        /// Handles firing <see cref="PropertyChanged"/> events as the task
        /// progresses.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected async override Task WatchTaskAsync(Task task)
        {
            bool alreadyCompleted = task.IsCompleted;

            await base.WatchTaskAsync(task);
            if (!alreadyCompleted && !(task.IsCanceled || task.IsFaulted))
            {
                RaisePropertyChanged(nameof(Result));
            }
        }
    }
}