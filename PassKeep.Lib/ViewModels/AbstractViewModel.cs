using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    public class AbstractViewModel : BindableBase, IViewModel
    {
        private State state = State.Constructed;

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
