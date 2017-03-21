using System;
using System.Windows.Input;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/> that can be
    /// takes an Action and a Func&lt;bool&gt; as the Execute and
    /// CanExecute delegates. It uses strongly typed parameters.
    /// </summary>
    /// <typeparam name="T">The type of parameter for the canExecute and execute methods.</typeparam>
    public class TypedCommand<T> : ICommand
    {
        /// <summary>
        /// An ActionCommand instance that does nothing.
        /// </summary>
        public static readonly TypedCommand<T> NoOp;
        static TypedCommand()
        {
            NoOp = new TypedCommand<T>(t => { });
        }

        private Func<T, bool> _canExecute;
        private Action<T> _actionToExecute;

        /// <summary>
        /// Simple constructor for the ActionCommand
        /// </summary>
        /// <param name="canExecute">A callback that returns whether this command
        /// can be currently executed</param>
        /// <param name="actionToExecute">A callback that represents the "meat" of
        /// this action</param>
        public TypedCommand(Func<T, bool> canExecute, Action<T> actionToExecute)
        {
            this._canExecute = canExecute;
            this._actionToExecute = actionToExecute;
        }

        /// <summary>
        /// A constructor to create a command that can always be executed.
        /// </summary>
        /// <param name="methodToExecute">A callback that represents the "meat" of
        /// this action</param>
        public TypedCommand(Action<T> methodToExecute)
            : this(t => true, methodToExecute)
        { }

        /// <summary>
        /// Evaluates whether this command is currently able to be executed.
        /// </summary>
        /// <param name="parameter">A parameter (not currently used)</param>
        /// <returns>Whether this command can execute</returns>
        public bool CanExecute(object parameter)
        {
            if (!(parameter is T || parameter == null))
            {
                return false;
            }

            return this._canExecute((T)parameter);
        }

        /// <summary>
        /// Raised in the event that the this command can now or can no longer
        /// be executed.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Executes this command with the given parameter.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (!(parameter is T))
            {
                throw new ArgumentException("parameter must match type of Command", "parameter");
            }

            this._actionToExecute((T)parameter);
        }
    }
}
