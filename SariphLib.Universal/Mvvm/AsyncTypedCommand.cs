using System;
using System.Threading.Tasks;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// A combination of <see cref="TypedCommand{T}"/> and <see cref="AsyncActionCommand"/>.
    /// An async command that has a strongly typed parameter.
    /// </summary>
    /// <typeparam name="TParameter">The type of the command's parameter.</typeparam>
    public sealed class AsyncTypedCommand<TParameter> : IAsyncCommand
    {
        private readonly Func<TParameter, bool> canExecute;
        private readonly Func<TParameter, Task> actionToExecute;

        /// <summary>
        /// Simple constructor for the ActionCommand
        /// </summary>
        /// <param name="canExecute">A callback that returns whether this command
        /// can be currently executed</param>
        /// <param name="actionToExecute">A callback that represents the "meat" of
        /// this action</param>
        public AsyncTypedCommand(Func<TParameter, bool> canExecute, Func<TParameter, Task> actionToExecute)
        {
            if (canExecute == null)
            {
                throw new ArgumentNullException(nameof(canExecute));
            }

            if (actionToExecute == null)
            {
                throw new ArgumentNullException(nameof(actionToExecute));
            }

            this.canExecute = canExecute;
            this.actionToExecute = actionToExecute;
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// A constructor to create a command that can always be executed.
        /// </summary>
        /// <param name="methodToExecute">A callback that represents the "meat" of
        /// this action</param>
        public AsyncTypedCommand(Func<TParameter, Task> methodToExecute)
            : this((parameter) => true, methodToExecute)
        { }

        /// <summary>
        /// Raised in the event that the this command can now or can no longer
        /// be executed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Bindable (INotifyPropertyChanged) representation of the executing task.
        /// </summary>
        public NotifyTaskCompletion Execution { get; private set; }

        /// <summary>
        /// Evaluates whether this command is currently able to be executed.
        /// </summary>
        /// <param name="parameter">Parameter for execution.</param>
        /// <returns>Whether this command can execute.</returns>
        public bool CanExecute(object parameter)
        {
            if (!(parameter is TParameter || parameter == null))
            {
                return false;
            }

            return this.canExecute((TParameter)parameter);
        }

        /// <summary>
        /// Synchronous execution by awaiting <see cref="ExecuteAsync(object)"/>.
        /// </summary>
        /// <param name="parameter">Parameter for execution.</param>
        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        /// <summary>
        /// Asynchronously executes the task represented by this command.
        /// </summary>
        /// <param name="parameter">Parameter for execution.</param>
        /// <returns></returns>
        public Task ExecuteAsync(object parameter)
        {
            if (!(parameter is TParameter))
            {
                throw new ArgumentException("parameter must match type of Command", nameof(parameter));
            }

            Execution = new NotifyTaskCompletion(actionToExecute((TParameter)parameter));
            return Execution.TaskCompletion;
        }

        /// <summary>
        /// Fires the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
