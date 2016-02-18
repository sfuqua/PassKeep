using System;
using System.Windows.Input;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/> that can be
    /// takes an Action and a Func&lt;bool&gt; as the Execute and
    /// CanExecute delegates. It discards the parameters provided.
    /// </summary>
    public class ActionCommand : ICommand
    {
        /// <summary>
        /// An ActionCommand instance that does nothing.
        /// </summary>
        public static readonly ActionCommand NoOp = new ActionCommand(() => { });

        private Func<bool> canExecute;
        private Action actionToExecute;

        /// <summary>
        /// Simple constructor for the ActionCommand
        /// </summary>
        /// <param name="canExecute">A callback that returns whether this command
        /// can be currently executed</param>
        /// <param name="actionToExecute">A callback that represents the "meat" of
        /// this action</param>
        public ActionCommand(Func<bool> canExecute, Action actionToExecute)
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
        /// Raised in the event that the this command can now or can no longer
        /// be executed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// A constructor to create a command that can always be executed.
        /// </summary>
        /// <param name="methodToExecute">A callback that represents the "meat" of
        /// this action</param>
        public ActionCommand(Action methodToExecute)
            : this(() => true, methodToExecute)
        { }

        /// <summary>
        /// Fires the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Evaluates whether this command is currently able to be executed.
        /// </summary>
        /// <param name="parameter">A parameter (not currently used)</param>
        /// <returns>Whether this command can execute</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute();
        }

        /// <summary>
        /// Executes this command with the given parameter.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            this.actionToExecute();
        }
    }
}
