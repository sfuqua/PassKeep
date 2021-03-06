﻿// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// <see cref="https://msdn.microsoft.com/en-us/magazine/dn630647.aspx"/>
    /// Thanks Stephen Cleary for the suggestion.
    /// </summary>
    public sealed class AsyncActionCommand : IAsyncCommand
    {
        private readonly Func<bool> canExecute;
        private readonly Func<Task> actionToExecute;

        /// <summary>
        /// Simple constructor for the ActionCommand
        /// </summary>
        /// <param name="canExecute">A callback that returns whether this command
        /// can be currently executed</param>
        /// <param name="actionToExecute">A callback that represents the "meat" of
        /// this action</param>
        public AsyncActionCommand(Func<bool> canExecute, Func<Task> actionToExecute)
        {
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            this.actionToExecute = actionToExecute ?? throw new ArgumentNullException(nameof(actionToExecute));
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// A constructor to create a command that can always be executed.
        /// </summary>
        /// <param name="methodToExecute">A callback that represents the "meat" of
        /// this action</param>
        public AsyncActionCommand(Func<Task> methodToExecute)
            : this(() => true, methodToExecute)
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
            return this.canExecute();
        }

        // Use framework type
        /// <summary>
        /// Synchronous execution by awaiting <see cref="ExecuteAsync(Object)"/>.
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
            Execution = new NotifyTaskCompletion(this.actionToExecute());
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
