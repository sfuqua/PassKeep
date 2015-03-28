using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using System;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace PassKeep
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        private IUnityContainer container;

        /// <summary>
        /// Initializes the singleton instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.container = new UnityContainer();
            ContainerBootstrapper.RegisterTypes(container);

            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        public Frame RootFrame
        {
            get
            {
                // Attempt to use the Window content
                Frame thisFrame = Window.Current.Content as Frame;
                if (thisFrame != null)
                {
                    return thisFrame;
                }

                // Hasn't been built yet... let's build it up
                thisFrame = new Frame
                {
                    CacheSize = 1
                };

                SuspensionManager.RegisterFrame(thisFrame, "AppFrame");
                thisFrame.Navigated += RootFrame_Navigated;

                Window.Current.Content = thisFrame;
                return thisFrame;
            }
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            RootView newView = e.Content as RootView;
            Dbg.Assert(newView != null, "The RootFrame should only navigate to a RootView");

            // Glean any ParameterOverrides that were passed in via the parameter
            ResolverOverride[] overrides;
            if (e.Parameter != null)
            {
                overrides = e.Parameter.GetType().GetRuntimeProperties()
                    .Select(prop => new ParameterOverride(
                        prop.Name,
                        new InjectionParameter(
                            prop.PropertyType,
                            prop.GetValue(e.Parameter)
                        )
                    )).ToArray();
            }
            else
            {
                overrides = new ResolverOverride[0];
            }
            newView.Container = this.container;
            newView.ViewModel = this.container.Resolve<IRootViewModel>(overrides);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
                //this.DebugSettings.EnableRedrawRegions = true;
                this.DebugSettings.IsBindingTracingEnabled = true;
            }
#endif

            if (this.RootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (this.RootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in this.RootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                this.RootFrame.ContentTransitions = null;
                this.RootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!this.RootFrame.Navigate(typeof(RootView),
                    new {
                        arguments = e.Arguments,
                        activationMode = ActivationMode.Regular,
                        openedFile = (StorageFile)null
                    }
                ))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when the app is activated due to a file open operation.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
                //this.DebugSettings.EnableRedrawRegions = true;
                this.DebugSettings.IsBindingTracingEnabled = true;
            }
#endif

            StorageFile openedFile = args.Files[0] as StorageFile;

            if (this.RootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (this.RootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in this.RootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                this.RootFrame.ContentTransitions = null;
                this.RootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!this.RootFrame.Navigate(typeof(RootView),
                    new
                    {
                        arguments = args.Verb,
                        activationMode = ActivationMode.File,
                        openedFile = openedFile
                    }
                ))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            else
            {
                // If the app was already running, open the file
                RootView view = this.RootFrame.Content as RootView;
                view.OpenFile(openedFile);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}