using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PassKeep.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PassKeep.ViewModels;
using PassKeep.Common;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Services;
using Windows.ApplicationModel;
using PassKeep.Lib.Services;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using PassKeep.Lib.Contracts.ViewModels;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace PassKeep
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class PassKeepApp : Application
    {
        private readonly ContainerHelper _containerHelper;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public PassKeepApp()
        {
            this.InitializeComponent();

            IUnityContainer container = new UnityContainer();
            ContainerBootstrapper.RegisterTypes(container);
            this._containerHelper = new ContainerHelper(container);

            this.Suspending += OnSuspending;
        }

        private async Task<Frame> getRootFrame(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                rootFrame = new Frame();
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other newActiveEntry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = await getRootFrame(args);
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!this._containerHelper.ResolveAndNavigate<RootView, IRootViewModel>(rootFrame))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            Frame rootFrame = await getRootFrame(args);
            Window.Current.Activate();

            ISearchViewModel searchArgs = this._container.Resolve<ISearchViewModel>(
                new ParameterOverride("query", args.QueryText)
                );

            if (rootFrame.Content == null)
            {
                // We searched globally, apparently, instead of in an app context.
                rootFrame.Navigate(
                    typeof(RootView),
                    //new MainViewModel(appSettings, ActivationMode.Search) { SearchViewModel = searchArgs });
                    this._container.Resolve<IRootViewModel>(
                        new ParameterOverride("activationMode", ActivationMode.Search),
                        new PropertyOverride("SearchViewModel", searchArgs)
                        )
                    );
            }
            else
            {
                RootView page = (RootView)rootFrame.Content;
                page.Search(searchArgs);
            }
        }

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = await getRootFrame(args);

            FileOpenViewModel openedFile = new FileOpenViewModel(appSettings, args.Files[0] as StorageFile);
            if (rootFrame.Content == null)
            {
                // App is not running, we were activated by Windows
                rootFrame.Navigate(typeof(MainPage), new MainViewModel(appSettings, ActivationMode.File) { FileOpenViewModel = openedFile });
            }
            else
            {
                MainPage page = (MainPage)rootFrame.Content;
                page.OpenFile(openedFile);
            }

            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
