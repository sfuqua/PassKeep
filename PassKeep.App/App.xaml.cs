using Microsoft.ApplicationInsights;
using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public static TelemetryClient TelemetryClient;

        private IUnityContainer container;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            App.TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();

            this.container = new UnityContainer();
            ContainerBootstrapper.RegisterTypes(container);

            IAppSettingsService settings = this.container.Resolve<IAppSettingsService>();
            this.RequestedTheme = settings.AppTheme;

            this.InitializeComponent();
            this.Suspending += OnSuspending;
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

                thisFrame.NavigationFailed += OnNavigationFailed;
                thisFrame.Navigated += RootFrame_Navigated;

                Window.Current.Content = thisFrame;
                return thisFrame;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
                this.DebugSettings.IsBindingTracingEnabled = true;
                this.DebugSettings.BindingFailed += (s, a) =>
                {
                    Dbg.Trace(a.Message);
                };
            }
#endif

            if (this.RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                this.RootFrame.Navigate(typeof(RootView),
                    new
                    {
                        arguments = e.Arguments,
                        activationMode = ActivationMode.Regular,
                        openedFile = (StorageFile)null
                    }
                );
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
            // Save state, e.g., which database file is open (we will prompt to unlock again on restore)
            deferral.Complete();
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
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
    }
}
