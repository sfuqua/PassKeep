using PassKeep.Lib.Contracts.Enums;
using SariphLib.Mvvm;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// A service that allows a ViewModel to notify the View of a long running
    /// operation that:
    ///  * Might need to modally block the UI with a progress indicator
    ///  * Might need to be cancellable
    /// </summary>
    public interface ITaskNotificationService : INotifyPropertyChanged
    {
        /// <summary>
        /// The current/topmost operation in progress.
        /// </summary>
        NotifyTaskCompletion CurrentTask { get; }

        /// <summary>
        /// A descriptor of the current operation.
        /// </summary>
        AsyncOperationType CurrentTaskType { get; }

        /// <summary>
        /// Adds an an operation that is externally cancellable via <paramref name="cts"/>.
        /// </summary>
        /// <param name="operation">The operation to add to the stack.</param>
        /// <param name="cts">A <see cref="CancellationTokenSource"/> that can be
        /// used to cancel the operation.</param>
        /// <param name="operationType">A classification of the operation.</param>
        void PushOperation(Task operation, CancellationTokenSource cts, AsyncOperationType operationType);

        /// <summary>
        /// Adds an operation that cannot be cancelled.
        /// </summary>
        /// <param name="operation">The operation to add to the stack.</param>
        /// <param name="operationType">A classification of the operation.</param>
        void PushOperation(Task operation, AsyncOperationType operationType);

        /// <summary>
        /// Attempts to cancel the current operation.
        /// </summary>
        /// <returns>Whether cancellation was successful.</returns>
        bool RequestCancellation();
    }
}
