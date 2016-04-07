using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using SariphLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A service that allows a ViewModel to notify the View of a long running
    /// operation that:
    ///  * Might need to modally block the UI with a progress indicator
    ///  * Might need to be cancellable
    /// </summary>
    public sealed class TaskNotificationService : BindableBase, ITaskNotificationService
    {
        private NotifyTaskCompletion currentTask;
        private AsyncOperationType currentTaskType;
        private CancellationTokenSource currentCts;

        /// <summary>
        /// The current/topmost operation in progress.
        /// </summary>
        public NotifyTaskCompletion CurrentTask
        {
            get { return this.currentTask; }
            private set
            {
                TrySetProperty(ref this.currentTask, value);
            }
        }

        /// <summary>
        /// A descriptor of the current operation.
        /// </summary>
        public AsyncOperationType CurrentTaskType
        {
            get { return this.currentTaskType; }
            private set
            {
                TrySetProperty(ref this.currentTaskType, value);
            }
        }

        /// <summary>
        /// Adds an an operation that is externally cancellable via <paramref name="cts"/>.
        /// </summary>
        /// <param name="operation">The operation to add to the stack.</param>
        /// <param name="cts">A <see cref="CancellationTokenSource"/> that can be
        /// used to cancel the operation.</param>
        /// <param name="operationType">A classification of the operation.</param>
        public void PushOperation(Task operation, CancellationTokenSource cts, AsyncOperationType operationType)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (cts == null)
            {
                throw new ArgumentNullException(nameof(cts));
            }

            if (HasTaskInProgress())
            {
                throw new InvalidOperationException("Cannot add a task, one is still running.");
            }

            this.CurrentTask = new NotifyTaskCompletion(operation);
            this.currentCts = cts;
            this.CurrentTaskType = operationType;
        }

        /// <summary>
        /// Adds an operation that cannot be cancelled.
        /// </summary>
        /// <param name="operation">The operation to add to the stack.</param>
        /// <param name="operationType">A classification of the operation.</param>
        public void PushOperation(Task operation, AsyncOperationType operationType)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (HasTaskInProgress())
            {
                throw new InvalidOperationException("Cannot add a task, one is still running.");
            }

            this.CurrentTask = new NotifyTaskCompletion(operation);
            this.currentCts = null;
            this.CurrentTaskType = operationType;
        }

        /// <summary>
        /// Attempts to cancel the current operation.
        /// </summary>
        /// <returns>Whether cancellation was successful.</returns>
        public bool RequestCancellation()
        {
            if (this.currentCts == null)
            {
                return false;
            }

            this.currentCts.Cancel();
            this.currentCts = null;

            return true;
        }

        /// <summary>
        /// Helper to determine whether a task is currently running.
        /// </summary>
        /// <returns>False if the current task is faulted/cancelled/completed, else true.</returns>
        private bool HasTaskInProgress()
        {
            if (this.CurrentTask == null)
            {
                return false;
            }

            return !this.CurrentTask.IsFaulted && !this.CurrentTask.IsCanceled &&
                !this.CurrentTask.IsCompleted;
        }
    }
}
