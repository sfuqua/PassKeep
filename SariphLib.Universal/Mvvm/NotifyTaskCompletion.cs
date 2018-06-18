// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.ComponentModel;
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
    public class NotifyTaskCompletion : INotifyPropertyChanged
    {
        /// <summary>
        /// Wraps the specified <see cref="Task"/> in a databindable object.
        /// </summary>
        /// <param name="task">The Task to wrap.</param>
        public NotifyTaskCompletion(Task task)
        {
            Task = task;
            TaskCompletion = WatchTaskAsync(task);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// A task that completes when <see cref="Task"/> does, but does not
        /// have results or propagate exceptions.
        /// </summary>
        public Task TaskCompletion { get; private set; }

        /// <summary>
        /// The wrapped task.
        /// </summary>
        public Task Task { get; private set; }

        #region Wrapped properties

        public TaskStatus Status { get { return Task.Status; } }

        public bool IsCompleted { get { return Task.IsCompleted; } }

        public bool IsNotCompleted { get { return !Task.IsCompleted; } }

        public bool IsSuccessfullyCompleted
        {
            get
            {
                return Task.Status == TaskStatus.RanToCompletion;
            }
        }

        public bool IsCanceled { get { return Task.IsCanceled; } }

        public bool IsFaulted { get { return Task.IsFaulted; } }

        public AggregateException Exception { get { return Task.Exception; } }

        public Exception InnerException
        {
            get
            {
                return Exception?.InnerException;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return InnerException?.Message;
            }
        }

        #endregion

        /// <summary>
        /// Self-explanatory.
        /// </summary>
        /// <param name="property"></param>
        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Handles firing <see cref="PropertyChanged"/> events as the task
        /// progresses.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected virtual async Task WatchTaskAsync(Task task)
        {
            bool alreadyCompleted = task.IsCompleted;

            try
            {
                await task;
            }
            // Swallow the exception because regardless of what happens to the task,
            // we're just propagating the information.
            catch { }

            if (alreadyCompleted)
            {
                return;
            }

            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                // If we have no event listeners, we're done.
                return;
            }

            // Basic status properties
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            // Failure-related properties
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            // Success-related properties
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
            }
        }
    }
}