// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Microsoft.ApplicationInsights;
using Microsoft.Practices.Unity;
using PassKeep.Framework;
using PassKeep.Framework.Reflection;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using SariphLib.Files;
using SariphLib.Diagnostics;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
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

        private TrackedPage trackedRoot;
        private IUnityContainer container;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            TelemetryClient = new TelemetryClient();

            this.container = new UnityContainer();
            ContainerBootstrapper.RegisterTypes(this.container);

            IAppSettingsService settings = this.container.Resolve<IAppSettingsService>();
            RequestedTheme = settings.AppTheme;

            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        public Frame RootFrame
        {
            get
            {
                // Attempt to use the Window content
                if (Window.Current.Content is Frame thisFrame)
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
        /// Handles bootstrapping the main frame and initial navigation.
        /// </summary>
        /// <param name="file">The file the app was opened with, or null (e.g. for regular launches).</param>
        protected void StartApp(StorageFile file)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
                DebugSettings.IsBindingTracingEnabled = true;
                DebugSettings.BindingFailed += (s, a) =>
                {
                    DebugHelper.Trace(a.Message);
                };
            }
#endif

            ActivationMode activationMode = (file == null ? ActivationMode.Regular : ActivationMode.File);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 800 / 1.5));

            if (RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                RootFrame.Navigate(typeof(RootView),
                    new NavigationParameter(
                        new
                        {
                            activationMode = activationMode,
                            openedFile = file
                        }
                    )
                );
            }
            else
            {
                RootView rootView = RootFrame.Content as RootView;
                DebugHelper.Assert(rootView != null);

                if (file != null)
                {
                    rootView.OpenFile(new StorageFileWrapper(file));
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            StartApp(file: null);
        }

        /// <summary>
        /// Invoked when the application is launched by opening a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            DebugHelper.Assert(args.Files.Count > 0);
            StartApp(args.Files[0] as StorageFile);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName + ", exception: " + e.Exception.ToString());
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
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            DebugHelper.Trace($"Suspending! Deadline: {e.SuspendingOperation.Deadline}. That is {e.SuspendingOperation.Deadline.Subtract(DateTime.Now).TotalSeconds} from now.");
            // Save state, e.g., which database file is open (we will prompt to unlock again on restore)
            RootView root = RootFrame.Content as RootView;
            root.HandleSuspend();
            deferral.Complete();
        }

        /// <summary>
        /// Handles resuming.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResuming(object sender, object e)
        {
            RootView root = RootFrame.Content as RootView;
            root.HandleResume();
        }

        private async void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            RootView newView = e.Content as RootView;
            DebugHelper.Assert(newView != null, "The RootFrame should only navigate to a RootView");

            // Build up the RootView's ViewModel and event handlers
            this.trackedRoot = new TrackedPage(newView, e.Parameter, this.container);
            await this.trackedRoot.InitialActivation;

            newView.Container = this.container;
        }
    }
}
