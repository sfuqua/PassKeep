using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;

namespace PassKeep.Framework
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
    }
}
