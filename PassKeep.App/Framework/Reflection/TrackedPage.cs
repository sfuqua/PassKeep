// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PassKeep.Framework.Reflection
{
    /// <summary>
    /// Represents a bundle of known information about a given page (e.g., wired event handlers)
    /// to promote re-use and easy cleanup.
    /// </summary>
    public sealed class TrackedPage
    {
        private readonly IEventLogger logger;
        private IViewModel viewModel;
        private IList<Tuple<EventInfo, Delegate>> autoHandlers;

        /// <summary>
        /// Wraps the provided <paramref name="page"/> in a new instance of this class, wiring up the ViewModel
        /// and attaching event handlers automatically, as well as injecting any missing dependencies.
        /// </summary>
        /// <param name="page">The page to wrap.</param>
        /// <param name="navigationParameter">The parameter used to navigate to the page, used in ViewModel construction.</param>
        /// <param name="container">An IoC container.</param>
        public TrackedPage(PassKeepPage page, object navigationParameter, IUnityContainer container)
        {
            this.logger = container.Resolve<IEventLogger>();
            Page = page;

            // container.BuildUp is not doing this for some reason
            Page.Logger = this.logger;
            Page.DatabaseCandidateFactory = container.Resolve<IDatabaseCandidateFactory>();
            this.viewModel = PageBootstrapper.GenerateViewModel(Page, navigationParameter, container, out Type viewType, out Type viewModelType);
            this.autoHandlers = PageBootstrapper.WireViewModelEventHandlers(Page, this.viewModel, viewType, viewModelType);

            if (this.viewModel != null)
            {
                InitialActivation = this.viewModel.ActivateAsync();
            }
            else
            {
                InitialActivation = Task.CompletedTask;
            }
        }

        /// <summary>
        /// Await this in order to proceed once the wrapped ViewModel is finished initializing.
        /// </summary>
        public Task InitialActivation
        {
            get;
            private set;
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
                Tuple<EventInfo, Delegate> autoHandler = this.autoHandlers[0];
                this.autoHandlers.RemoveAt(0);

                autoHandler.Item1.RemoveEventHandler(this.viewModel, autoHandler.Item2);
                DebugHelper.Trace($"Removed auto-EventHandler {autoHandler.Item2} for event {autoHandler.Item1.Name}");
            }
        }

        /// <summary>
        /// Await this to continue when all event handlers and the wrapped ViewModel are
        /// cleaned up.
        /// </summary>
        /// <returns></returns>
        public async Task CleanupAsync()
        {
            UnregisterViewModelEventHandlers();
            this.autoHandlers = null;
            if (this.viewModel != null)
            {
                await this.viewModel.SuspendAsync();
                this.viewModel = null;
            }
            Page = null;
        }
    }
}
