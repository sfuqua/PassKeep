using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// A class that provides abstracted DI container helper methods.
    /// </summary>
    public class ContainerHelper
    {
        public const string GroupDetailsViewNew = "new-group";
        public const string GroupDetailsViewExisting = "existing-group";
        public const string EntryDetailsViewNew = "new-entry";
        public const string EntryDetailsViewExisting = "existing-entry";

        private IUnityContainer _container;

        public ContainerHelper(IUnityContainer container)
        {
            this._container = container;
        }
    }
}
