using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    public class AbstractViewModel : BindableBase, IViewModel
    {
        private bool active = false;

        public virtual Task ActivateAsync()
        {
            if (this.active)
            {
                throw new InvalidOperationException("Already active!");
            }

            this.active = true;
            return Task.CompletedTask;
        }

        public virtual Task SuspendAsync()
        {
            if (!this.active)
            {
                throw new InvalidOperationException("Already suspended!");
            }

            this.active = false;
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
    }
}
