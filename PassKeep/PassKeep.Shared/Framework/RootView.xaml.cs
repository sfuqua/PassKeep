using Microsoft.Practices.Unity;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootView : Page
    {
        private CancellationTokenSource activeLoadingCts;

        public RootView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Allows access to the IoC container used by the RootView.
        /// </summary>
        public IUnityContainer Container
        {
            private get;
            set;
        }

        /// <summary>
        /// Allows access to the ViewModel.
        /// </summary>
        public IRootViewModel ViewModel
        {
            private get;
            set;
        }

        /// <summary>
        /// Navigates to the proper view for opening a database file.
        /// </summary>
        /// <param name="file">The file being opened.</param>
        public void OpenFile(StorageFile file)
        {
            Debug.WriteLine("Navigating RootView to Database Unlocker...");
            this.contentFrame.Navigate(typeof(DatabaseUnlockView),
                new
                {
                    file = file,
                    isSampleFile = false
                }
            );
        }

        /// <summary>
        /// Handles initialization logic when the RootView is first navigated to.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Use ActivationMode to decide how to navigate the ContentFrame
            switch(this.ViewModel.ActivationMode)
            {
                case ActivationMode.Regular:
                    // Load the welcome hub
                    Debug.WriteLine("Navigating RootView to Dashboard...");
                    this.contentFrame.Navigate(typeof(DashboardView));
                    break;
                case ActivationMode.File:
                    // Load the DatabaseView
                    OpenFile(this.ViewModel.OpenedFile);
                    break;
                default:
                    throw new NotImplementedException();
            }

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            #if WINDOWS_APP

            SettingsPane.GetForCurrentView().CommandsRequested += SettingsCommandsRequested;
            
            #endif
        }

        #if WINDOWS_APP

        /// <summary>
        /// Handles populating commands for the Settings pane
        /// </summary>
        /// <param name="sender">The SettingsPane requesting commands.</param>
        /// <param name="args">EventArgs for the request.</param>
        private void SettingsCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // TODO: Configuration settings
            // TODO: Help

            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Feedback", "Ideas and Bugs",
                    async cmd => 
                        await Launcher.LaunchUriAsync(new Uri("mailto:passkeep@outlook.com"))
                )
            );
        }

        #endif

        /// <summary>
        /// Handles KeyDown events for the current window.
        /// </summary>
        /// <param name="sender">The CoreWindow that dispatched the event.</param>
        /// <param name="args">KeyEventArgs for the event.</param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            CoreVirtualKeyStates ctrlState = sender.GetKeyState(VirtualKey.Control);
            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                // Handle accelerator (Ctrl) hotkeys
                switch (args.VirtualKey)
                {
                    case VirtualKey.O:
                        // Prompt to open a file
                        args.Handled = true;
                        break;
                    default:
                        args.Handled = ((PassKeepPage)this.contentFrame.Content).HandleAcceleratorKey(args.VirtualKey);
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for when the ContentFrame's pages starts a blocking load operation.
        /// </summary>
        /// <param name="sender">Presumably the ContentFrame's content.</param>
        /// <param name="e">EventArgs for the load operation.</param>
        private void ContentFrameStartedLoading(object sender, LoadingStartedEventArgs e)
        {
            this.loadingPane.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.loadingText.Text = e.Text;
            this.activeLoadingCts = e.Cts;

            // TODO: Handle determinate loads
            this.loadingStatusDeterminate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.loadingStatusIndeterminate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.loadingStatusIndeterminate.IsActive = true;

            this.contentFrame.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for when the ContentFrame's pages have terminated a blocking load.
        /// </summary>
        /// <param name="sender">Presumably the ContentFrame's content.</param>
        /// <param name="e">EventArgs for the operation.</param>
        private void ContentFrameDoneLoading(object sender, EventArgs e)
        {
            this.activeLoadingCts = null;

            this.loadingStatusDeterminate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.loadingStatusIndeterminate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.loadingPane.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.contentFrame.IsEnabled = true;
        }

        #region Declaratively bound event handlers

        /// <summary>
        /// Invoked when the content Frame of the RootView is Navigating.
        /// </summary>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void contentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Debug.Assert(sender == this.contentFrame);

            PassKeepPage previousContent = this.contentFrame.Content as PassKeepPage;
            if (previousContent != null)
            {
                // Abort any current load operation
                if (this.activeLoadingCts != null)
                {
                    this.activeLoadingCts.Cancel();
                }

                // Tear down loading event handlers
                previousContent.StartedLoading -= ContentFrameStartedLoading;
                previousContent.DoneLoading -= ContentFrameDoneLoading;
            }
        }

        /// <summary>
        /// Invoked when the content Frame of the RootView is done navigating..
        /// </summary>
        /// <remarks>
        /// Hooks up the new content Page's IOC logic.
        /// </remarks>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Build up the new PassKeep Page
            PassKeepPage newContent = e.Content as PassKeepPage;
            Debug.Assert(newContent != null, "The contentFrame should always navigate to a PassKeepPage");

            // Hook up loading event handlers
            newContent.StartedLoading += ContentFrameStartedLoading;
            newContent.DoneLoading += ContentFrameDoneLoading;

            // Wire up the ViewModel
            // First, we figure out the ViewModel interface type
            Type viewType = newContent.GetType();
            Type viewBaseType = viewType.GetTypeInfo().BaseType;
            Type genericPageType = viewBaseType.GetTypeInfo().BaseType;
            Type viewModelType = genericPageType.GenericTypeArguments[0];

            // Next, we glean any ParameterOverrides that were passed in via the parameter
            ResolverOverride[] overrides;
            if (e.Parameter != null)
            {
                overrides = e.Parameter.GetType().GetRuntimeProperties()
                    .Select(prop => new ParameterOverride(
                        prop.Name,
                        prop.GetValue(e.Parameter)
                    )).ToArray();
            }
            else
            {
                overrides = new ResolverOverride[0];
            }

            // Finally, we resolve the ViewModel (with overrides) from the container
            IViewModel viewModel = (IViewModel)this.Container.Resolve(viewModelType, overrides);

            // Now that we have the ViewModel, attach it to the new View
            newContent.DataContext = viewModel;
            Debug.WriteLine("Successfully wired DataContext ViewModel to new RootFrame content!");
        }

        #endregion
    }
}
