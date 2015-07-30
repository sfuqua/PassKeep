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
        public static readonly ActionCommand NoOp;
        static ActionCommand()
        {
            NoOp = new ActionCommand(() => { });
        }

        private Func<bool> _canExecute;
        private Action _actionToExecute;

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
                throw new ArgumentNullException("canExecute");
            }

            if (actionToExecute == null)
            {
                throw new ArgumentNullException("actionToExecute");
            }

            this._canExecute = canExecute;
            this._actionToExecute = actionToExecute;
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// A constructor to create a command that can always be executed.
        /// </summary>
        /// <param name="methodToExecute">A callback that represents the "meat" of
        /// this action</param>
        public ActionCommand(Action methodToExecute)
            : this(() => true, methodToExecute)
        { }

        /// <summary>
        /// Evaluates whether this command is currently able to be executed.
        /// </summary>
        /// <param name="parameter">A parameter (not currently used)</param>
        /// <returns>Whether this command can execute</returns>
        public bool CanExecute(object parameter)
        {
            return this._canExecute();
        }

        /// <summary>
        /// Raised in the event that the this command can now or can no longer
        /// be executed.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Executes this command with the given parameter.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            this._actionToExecute();
        }
    }
}
