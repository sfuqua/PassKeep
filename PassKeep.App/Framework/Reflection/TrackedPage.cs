using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PassKeep.Framework.Reflection
{
    /// <summary>
    /// Represents a bundle of known information about a given page (e.g., wired event handlers)
    /// to promote re-use and easy cleanup.
    /// </summary>
    public sealed class TrackedPage : IDisposable
    {
        private IViewModel viewModel;
        private IList<Tuple<EventInfo, Delegate>> autoHandlers;

        /// <summary>
        /// Wraps the provided <paramref name="page"/> in a new instance of this class, wiring up the ViewModel
        /// and attaching event handlers automatically.
        /// </summary>
        /// <param name="page">The page to wrap.</param>
        /// <param name="navigationParameter">The parameter used to navigate to the page, used in ViewModel construction.</param>
        /// <param name="container">An IoC container.</param>
        public TrackedPage(PassKeepPage page, object navigationParameter, IUnityContainer container)
        {
            this.Page = page;

            Type viewType, viewModelType;
            this.viewModel = PageBootstrapper.GenerateViewModel(this.Page, navigationParameter, container, out viewType, out viewModelType);
            this.autoHandlers = PageBootstrapper.WireViewModelEventHandlers(this.Page, this.viewModel, viewType, viewModelType);

            if (this.viewModel != null)
            {
                this.viewModel.Activate();
            }
        }

        /// <summary>
        /// The wrapped page.
        /// </summary>
        public PassKeepPage Page
        {
            get;
            private set;
        }

        /// <summary>
        /// Unregister any event handlers we set up automatically in construction.
        /// </summary>
        private void UnregisterViewModelEventHandlers()
        {
            while (this.autoHandlers.Count > 0)
            {
                Tuple<EventInfo, Delegate> autoHandler = autoHandlers[0];
                autoHandlers.RemoveAt(0);

                autoHandler.Item1.RemoveEventHandler(this.viewModel, autoHandler.Item2);
                Dbg.Trace($"Removed auto-EventHandler {autoHandler.Item2} for event {autoHandler.Item1.Name}");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state
                    UnregisterViewModelEventHandlers();
                    this.autoHandlers = null;
                    if (this.viewModel != null)
                    {
                        this.viewModel.Suspend();
                        this.viewModel = null;
                    }
                    this.Page = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TrackedPage() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
