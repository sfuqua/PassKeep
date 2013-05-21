using PassKeep.Common;
using PassKeep.Models;
using System;
using System.Collections.ObjectModel;

namespace PassKeep.ViewModels
{
    public abstract class ViewModelBase : BindableBase
    {
        public ConfigurationViewModel Settings
        {
            get;
            private set;
        }

        public ViewModelBase(ConfigurationViewModel appSettings)
        {
            Settings = appSettings;
        }
    }
}
