using Microsoft.Practices.Unity;

namespace PassKeep.Framework
{
    /// <summary>
    /// A class that provides abstracted DI container helper methods.
    /// </summary>
    public class ContainerHelper
    {
        private IUnityContainer _container;

        public ContainerHelper(IUnityContainer container)
        {
            this._container = container;
        }
    }
}
