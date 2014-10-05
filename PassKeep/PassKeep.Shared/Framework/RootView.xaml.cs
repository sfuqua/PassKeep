using Microsoft.Practices.Unity;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#if WINDOWS_APP

using Windows.UI.ApplicationSettings;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Resources;

#endif

namespace PassKeep.Framework
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootView : RootPassKeepPage
    {
        private const string FeedbackDescriptionResourceKey = "FeedbackDescription";
        private const string ContactEmailResourceKey = "ContactEmail";

        private CancellationTokenSource activeLoadingCts;

        // A list of delegates that were auto-attached (by convention) to ViewModel events, so that they
        // can be cleaned up later.
        private readonly IList<Tuple<EventInfo, Delegate>> autoMethodHandlers = new List<Tuple<EventInfo, Delegate>>();
        private IViewModel contentViewModel;

        // Tracks ICommandBarElements added to the appbar for each new View, to remove later.
        private readonly IList<ICommandBarElement> primaryCommandAdditions = new List<ICommandBarElement>();
        private readonly IList<ICommandBarElement> secondaryCommandAdditions = new List<ICommandBarElement>();

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
                    file = new StorageFileDatabaseCandidate(file),
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
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            // TODO: Configuration settings
            // TODO: Help
            
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedbacak",
                    loader.GetString(RootView.FeedbackDescriptionResourceKey),
                    async cmd => 
                        await Launcher.LaunchUriAsync(
                            new Uri(
                                String.Format(
                                    "mailto:{0}",
                                    loader.GetString(RootView.ContactEmailResourceKey)
                                )
                            )
                        )
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
                        this.Open_AppBarButton_Click(null, null);
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

        /// <summary>
        /// Event handler for when the ContentFrame's pages have indicated primary AppBar commands
        /// are available.
        /// </summary>
        /// <param name="sender">The Page indicating commands are available.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void ContentFramePrimaryCommandsAvailable(PassKeepPage sender, EventArgs e)
        {
            CommandBar appBar = this.BottomAppBar as CommandBar;
            RootView.ResetAppBarAdditions(
                this.primaryCommandAdditions,
                sender.GetPrimaryCommandBarElements,
                appBar.PrimaryCommands
            );
        }

        /// <summary>
        /// Event handler for when the ContentFrame's pages have indicated secondary AppBar commands
        /// are available.
        /// </summary>
        /// <param name="sender">The Page indicating commands are available.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void ContentFrameSecondaryCommandsAvailable(PassKeepPage sender, EventArgs e)
        {
            CommandBar appBar = this.BottomAppBar as CommandBar;
            RootView.ResetAppBarAdditions(
                this.secondaryCommandAdditions,
                sender.GetSecondaryCommandBarElements,
                appBar.SecondaryCommands
            );
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

                // Tear down app bar event handlers
                if (!previousContent.PrimaryCommandsImmediatelyAvailable)
                {
                    previousContent.PrimaryCommandsAvailable -= ContentFramePrimaryCommandsAvailable;
                }
                if (!previousContent.SecondaryCommandsImmediatelyAvailable)
                {
                    previousContent.SecondaryCommandsAvailable -= ContentFrameSecondaryCommandsAvailable;
                }

                // Unregister any event handlers we set up automatically
                while (this.autoMethodHandlers.Count > 0)
                {
                    var autoHandler = this.autoMethodHandlers[0];
                    this.autoMethodHandlers.RemoveAt(0);

                    autoHandler.Item1.RemoveEventHandler(this.contentViewModel, autoHandler.Item2);
                    Debug.WriteLine("Removed auto-EventHandler {0} for event {1}", autoHandler.Item2, autoHandler.Item1.Name);
                }

                // Clean up appbar
                CommandBar appBar = this.BottomAppBar as CommandBar;
                RootView.CleanupAppBarAdditions(this.primaryCommandAdditions, appBar.PrimaryCommands);
                RootView.CleanupAppBarAdditions(this.secondaryCommandAdditions, appBar.SecondaryCommands);
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
            // Dismiss the AppBar on navigate
            this.BottomAppBar.IsSticky = false;
            this.BottomAppBar.IsOpen = false;

            // Build up the new PassKeep Page
            PassKeepPage newContent = e.Content as PassKeepPage;
            Debug.Assert(newContent != null, "The contentFrame should always navigate to a PassKeepPage");

            // Hook up the command bar notification event handlers
            newContent.BottomAppBar = this.BottomAppBar;
            if (newContent.PrimaryCommandsImmediatelyAvailable)
            {
                ContentFramePrimaryCommandsAvailable(newContent, null);
            }
            else
            {
                newContent.PrimaryCommandsAvailable += ContentFramePrimaryCommandsAvailable;
            }

            if (newContent.SecondaryCommandsImmediatelyAvailable)
            {
                ContentFrameSecondaryCommandsAvailable(newContent, null);
            }
            else
            {
                newContent.SecondaryCommandsAvailable += ContentFrameSecondaryCommandsAvailable;
            }

            // Hook up loading event handlers
            newContent.StartedLoading += ContentFrameStartedLoading;
            newContent.DoneLoading += ContentFrameDoneLoading;

            // Wire up the ViewModel
            // First, we figure out the ViewModel interface type
            Type viewType = newContent.GetType();
            Type viewBaseType = viewType.GetTypeInfo().BaseType;
            Type genericPageType = viewBaseType.GetTypeInfo().BaseType;
            Type viewModelType = genericPageType.GenericTypeArguments[0];

            if (e.Parameter != null)
            {
                NavigationParameter parameter = e.Parameter as NavigationParameter;
                Debug.Assert(parameter != null);

                ResolverOverride[] overrides = parameter.DynamicParameters.ToArray();

                // We resolve the ViewModel (with overrides) from the container
                if (String.IsNullOrEmpty(parameter.ConcreteTypeKey))
                {
                    this.contentViewModel = (IViewModel)this.Container.Resolve(viewModelType, overrides);
                }
                else
                {
                    this.contentViewModel = 
                        (IViewModel)this.Container.Resolve(viewModelType, parameter.ConcreteTypeKey, overrides);
                }
            }
            else
            {
                this.contentViewModel = (IViewModel)this.Container.Resolve(viewModelType);
            }

            // Wire up any events on the ViewModel to conventionally named handles on the View
            Debug.Assert(this.autoMethodHandlers.Count == 0);
            IEnumerable<EventInfo> vmEvents = viewModelType.GetRuntimeEvents();
            foreach (EventInfo evt in vmEvents)
            {
                Type handlerType = evt.EventHandlerType;
                MethodInfo invokeMethod = handlerType.GetRuntimeMethods().First(method => method.Name == "Invoke");

                // By convention, auto-handlers will be named "EventNameHandler"
                string handlerName = evt.Name + "Handler";
                Type[] parameterTypes = invokeMethod.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

                // Try to fetch a method on the View that matches the event name, with the right parameters
                MethodInfo candidateHandler = viewType.GetRuntimeMethod(
                    handlerName,
                    parameterTypes
                );

                // If we got a matching method, hook it up!
                if (candidateHandler != null)
                {
                    Delegate handlerDelegate = candidateHandler.CreateDelegate(handlerType, newContent);
                    evt.AddEventHandler(this.contentViewModel, handlerDelegate);

                    // Save the delegate and the event for later, so we can unregister when we navigate away
                    this.autoMethodHandlers.Add(new Tuple<EventInfo, Delegate>(evt, handlerDelegate));

                    Debug.WriteLine("Attached auto-EventHandler {0} for event {1}", handlerDelegate, evt);
                }
            }

            // Finally, attach the ViewModel to the new View
            newContent.DataContext = this.contentViewModel;
            Debug.WriteLine("Successfully wired DataContext ViewModel to new RootFrame content!");
        }

        /// <summary>
        /// Handles displaying a FileOpenPicker from anywhere in the application.
        /// </summary>
        /// <param name="sender">The AppBarButton that triggered this.</param>
        /// <param name="e">EventARgs for the click.</param>
        private async void Open_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await PickFile(
                file =>
                {
                    OpenFile(file);
                }
            );
        }

        #endregion

        /// <summary>
        /// Handles resetting a list of command bar actions.
        /// </summary>
        /// <param name="additionTracker">The list used for tracking new commands.</param>
        /// <param name="additionGenerator">The function used to generate new commands.</param>
        /// <param name="commandBarList">The listing off of the CommandBar to add to or clear.</param>
        private static void ResetAppBarAdditions(
            IList<ICommandBarElement> additionTracker,
            Func<IList<ICommandBarElement>> additionGenerator,
            IObservableVector<ICommandBarElement> commandBarList
        )
        {
            RootView.CleanupAppBarAdditions(additionTracker, commandBarList);

            // Add new commands
            IList<ICommandBarElement> commands = additionGenerator();
            if (commands != null)
            {
                foreach (ICommandBarElement command in commands)
                {
                    additionTracker.Add(command);
                    commandBarList.Add(command);
                }
            }
        }

        /// <summary>
        /// Handles cleaning up any additions to a command bar action list.
        /// </summary>
        /// <param name="additionTracker">The list used for tracking new commands.</param>
        /// <param name="commandBarList">The listing off of the CommandBar to add to or clear.</param>
        private static void CleanupAppBarAdditions(
            IList<ICommandBarElement> additionTracker,
            IObservableVector<ICommandBarElement> commandBarList
        )
        {
            // Clear existing commands if necessary
            if (additionTracker.Count > 0)
            {
                foreach (ICommandBarElement command in additionTracker)
                {
                    commandBarList.Remove(command);
                }

                additionTracker.Clear();
            }
        }

        /// <summary>
        /// Handles showing or hiding AppBarButtons based on their IsEnabled state.
        /// </summary>
        /// <param name="commandBarList">The listing off of the CommandBar to update.</param>
        /// <remarks>
        /// This helps work around an apparent bug in the XAML framework that keeps IsEnabledChanged
        /// events from firing when the CommandBar is offscreen, preventing bindings from functioning as expected.
        /// </remarks>
        private static void UpdateAppBarButtonVisibilities(
            IObservableVector<ICommandBarElement> commandBarList
        )
        {
            foreach (ICommandBarElement element in commandBarList)
            {
                AppBarButton button = element as AppBarButton;
                if (button != null)
                {
                    button.HideWhenDisabled(); 
                }
            }
        }

        /// <summary>
        /// Handles button visibility when the CommandBar is opened.
        /// </summary>
        /// <param name="sender">The CommandBar itself.</param>
        /// <param name="e">Who knows?</param>
        private void CommandBar_Opened(object sender, object e)
        {
            CommandBar bar = sender as CommandBar;
            if (bar == null)
            {
                throw new ArgumentNullException("sender");
            }

            UpdateAppBarButtonVisibilities(bar.PrimaryCommands);
            UpdateAppBarButtonVisibilities(bar.SecondaryCommands);
        }
    }
}
