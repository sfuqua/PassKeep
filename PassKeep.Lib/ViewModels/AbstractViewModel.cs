// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;

namespace PassKeep.Lib.ViewModels
{
    public class AbstractViewModel : BindableBase, IViewModel
    {
        private State state = State.Constructed;

        /// <summary>
        /// Used to log events for debugging.
        /// </summary>
        public IEventLogger Logger { get; set; }

        public virtual Task ActivateAsync()
        {
            if (this.state == State.Activated)
            {
                throw new InvalidOperationException("Already active!");
            }

            this.state = State.Activated;
            return Task.CompletedTask;
        }

        public virtual Task SuspendAsync()
        {
            if (this.state == State.Suspended)
            {
                throw new InvalidOperationException("Already suspended!");
            }

            this.state = State.Suspended;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs the current type name and method.
        /// </summary>
        /// <param name="methodName">The method name to log.</param>
        protected void LogCurrentFunction([CallerMemberName] string methodName = null)
        {
            LogEventWithContext("MethodTrace", null, EventVerbosity.Info, methodName);
        }

        /// <summary>
        /// Logs an event with context about the calling function. Verbosity is 
        /// </summary>
        /// <param name="eventName">Event to log.</param>
        /// <param name="fields">Fields to include with the event.</param>
        /// <param name="verbosity">Verbosity for the logged event.</param>
        /// <param name="context">Context to include in the log.</param>
        protected void LogEventWithContext(string eventName, LoggingFields fields = null, EventVerbosity verbosity = EventVerbosity.Info, [CallerMemberName] string context = null)
        {
            fields = fields ?? new LoggingFields();
            fields.AddString("Context", context);
            Logger?.LogEvent(eventName, fields, verbosity);
        }

        /// <summary>
        /// Saves state when app is being suspended.
        /// </summary>
        public virtual void HandleAppSuspend() { }

        /// <summary>
        /// Restores state when app is resumed.
        /// </summary>
        public virtual void HandleAppResume() { }

        /// <summary>
        /// Tracks activation state.
        /// </summary>
        private enum State
        {
            /// <summary>
            /// The VM has been created but not activated.
            /// </summary>
            Constructed,

            /// <summary>
            /// The VM has started (or completed) its activation.
            /// </summary>
            Activated,

            /// <summary>
            /// The VM has been suspended.
            /// </summary>
            Suspended
        }
    }
}
