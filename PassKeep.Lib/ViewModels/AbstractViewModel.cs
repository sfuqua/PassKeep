using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.ViewModels
{
    public class AbstractViewModel : BindableBase, IViewModel
    {
        private bool active = false;

        public virtual void Activate()
        {
            if (this.active)
            {
                throw new InvalidOperationException("Already active!");
            }

            this.active = true;
        }

        public virtual void Suspend()
        {
            if (!this.active)
            {
                throw new InvalidOperationException("Already suspended!");
            }

            this.active = false;
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
