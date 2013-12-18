using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Lib.Contracts.Services;
using System;

namespace PassKeep.ViewModels
{
    public abstract class ViewModelBase : BindableBase
    {
        IUnityContainer iocContainer
        {
            protected get;
            private set;
        }

        IAppSettingsService SettingsService
        {
            protected get;
            private set;
        }

        public ViewModelBase(IUnityContainer iocContainer)
        {
            if (iocContainer == null)
            {
                throw new ArgumentNullException("iocContainer");
            }
        }
    }
}
