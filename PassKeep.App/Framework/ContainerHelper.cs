using Microsoft.Practices.Unity;
using SariphLib.Mvvm;
using System;

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
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this._container = container;
        }

        /// <summary>
        /// Gets the current synchronization context.
        /// </summary>
        /// <returns>The current sync context.</returns>
        public ISyncContext GetSyncContext()
        {
            return this._container.Resolve<ISyncContext>();
        }
    }
}
