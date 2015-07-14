using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Contracts.ViewModels;
using PassKeep.Framework.Messages;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.Views;
using PassKeep.Views.Flyouts;
using SariphLib.Infrastructure;
using SariphLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
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
    public sealed partial class RootView : RootPassKeepPage
    {
        public readonly RelayCommand ContentBackCommand;

        private const string FeedbackDescriptionResourceKey = "FeedbackDescription";
        private const string ContactEmailResourceKey = "ContactEmail";

        private readonly string LockedGlyph = "&#xE1F6;";
        private readonly string UnlockedGlyph = "&#xE1F7;";

        private SystemNavigationManager systemNavigationManager;
        private CancellationTokenSource activeLoadingCts;

        // A list of delegates that were auto-attached (by convention) to ViewModel events, so that they
        // can be cleaned up later.
        private readonly Dictionary<PassKeepPage, IList<Tuple<EventInfo, Delegate>>> autoMethodHandlers = new Dictionary<PassKeepPage, IList<Tuple<EventInfo, Delegate>>>();
        private IViewModel contentViewModel;

        // Whether the last navigation was caused by a SplitView nav button
        private bool splitViewNavigation = false;

        private IClipboardClearTimerViewModel clipboardViewModel;

        private readonly object splitViewSyncRoot = new object();

        public RootView()
        {
            this.InitializeComponent();

            this.ContentBackCommand = new RelayCommand(
                () => { this.contentFrame.GoBack(); },
                () => this.contentFrame.CanGoBack
            );

            this.MessageBus = new MessageBus();
            BootstrapMessageSubscriptions(typeof(DatabaseCandidateMessage), typeof(DatabaseOpenedMessage));
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

        #region Message handlers

        public Task HandleDatabaseCandidateMessage(DatabaseCandidateMessage message)
        {
            this.ViewModel.CandidateFile = message.File;
            return Task.FromResult(0);
        }

        public Task HandleDatabaseOpenedMessage(DatabaseOpenedMessage message)
        {
            this.ViewModel.DecryptedDatabase = message.ViewModel;
            return Task.FromResult(0);
        }

        #endregion

        /// <summary>
        /// Navigates to the proper view for opening a database file.
        /// </summary>
        /// <param name="file">The file being opened.</param>
        /// <param name="isSample">Whether we are unlocking a sample file.</param>
        public void OpenFile(IStorageFile file, bool isSample = false)
        {
            Dbg.Trace("Navigating RootView to Database Unlocker...");
            this.contentFrame.Navigate(typeof(DatabaseUnlockView),
                new NavigationParameter(
                    new {
                        file = new StorageFileDatabaseCandidate(file),
                        isSampleFile = isSample
                    }
                )
            );
        }

        /// <summary>
        /// Handles initialization logic when the RootView is first navigated to.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            this.systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            // Use ActivationMode to decide how to navigate the ContentFrame
            switch(this.ViewModel.ActivationMode)
            {
                case ActivationMode.Regular:
                    // Load the welcome hub
                    Dbg.Trace("Navigating RootView to Dashboard...");
                    this.contentFrame.Navigate(typeof(DashboardView));
                    break;
                case ActivationMode.File:
                    // Load the DatabaseView
                    OpenFile(this.ViewModel.CandidateFile);
                    break;
                default:
                    throw new NotImplementedException();
            }

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            this.clipboardViewModel = this.Container.Resolve<IClipboardClearTimerViewModel>();
            this.clipboardViewModel.TimerComplete += ClipboardClearTimer_Complete;
        }

        /// <summary>
        /// Handles navigating when the system (chrome back button, HW back button) requests a back.
        /// </summary>
        /// <param name="sender">Null.</param>
        /// <param name="e">EventArgs for the back request event.</param>
        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            SystemNavigationManager navManager = sender as SystemNavigationManager;
            if (this.contentFrame != null && this.contentFrame.CanGoBack)
            {
                this.contentFrame.GoBack();
                this.ContentBackCommand.RaiseCanExecuteChanged();

                if (this.systemNavigationManager != null)
                {
                    this.systemNavigationManager.AppViewBackButtonVisibility =
                        (this.ContentBackCommand.CanExecute(null) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
                }

                e.Handled = true;
            }
        }


        /// <summary>
        /// Unhooks event handlers for the page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.clipboardViewModel.TimerComplete -= ClipboardClearTimer_Complete;
        }

        /// <summary>
        /// Handles KeyDown events for the current window.
        /// </summary>
        /// <param name="sender">The CoreWindow that dispatched the event.</param>
        /// <param name="args">KeyEventArgs for the event.</param>
        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
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
                        await PickFile(
                            file =>
                            {
                                OpenFile(file);
                            }
                        );
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
            if (this.loadingPane == null)
            {
                this.loadingPane = (Grid)FindName("loadingPane");
            }

            this.loadingPane.Visibility = Visibility.Visible;
            this.loadingText.Text = e.Text;
            this.activeLoadingCts = e.Cts;

            // TODO: Handle determinate loads
            this.loadingStatusDeterminate.Visibility = Visibility.Collapsed;
            this.loadingStatusIndeterminate.Visibility = Visibility.Visible;
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
            if (this.loadingPane == null)
            {
                this.loadingPane = (Grid)FindName("loadingPane");
            }

            this.activeLoadingCts = null;

            this.loadingStatusDeterminate.Visibility = Visibility.Collapsed;
            this.loadingStatusIndeterminate.Visibility = Visibility.Collapsed;
            this.loadingPane.Visibility = Visibility.Collapsed;

            this.contentFrame.IsEnabled = true;
        }

        /// <summary>
        /// Creates a ViewModel for a new page and hooks up various event handlers.
        /// </summary>
        /// <param name="newContent">A page that was just navigated to.</param>
        /// <param name="navParameter">The parameter that was passed with the navigation.</param>
        private void HandleNewFrameContent(PassKeepPage newContent, object navParameter)
        {
            this.ContentBackCommand.RaiseCanExecuteChanged();
            if (this.systemNavigationManager != null)
            {
                this.systemNavigationManager.AppViewBackButtonVisibility =
                    (this.ContentBackCommand.CanExecute(null) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            }

            INestingPage newHostingContent = newContent as INestingPage;
            if (newHostingContent != null)
            {
                TrackFrame(newHostingContent.ContentFrame);
            }

            // Set up the ClipboardClearViewModel and MessageBus
            newContent.MessageBus = this.MessageBus;
            newContent.ClipboardClearViewModel = this.clipboardViewModel;

            // Hook up loading event handlers
            newContent.StartedLoading += ContentFrameStartedLoading;
            newContent.DoneLoading += ContentFrameDoneLoading;

            // Wire up the ViewModel
            // First, we figure out the ViewModel interface type
            Type viewType = newContent.GetType();
            Type viewBaseType = viewType.GetTypeInfo().BaseType;

            if (viewBaseType.Equals(typeof(PassKeepPage)))
            {
                // This is just a PassKeepPage, not a generic type. No ViewModel construction is necessary.
                Dbg.Assert(navParameter == null);
                this.autoMethodHandlers[newContent] = new List<Tuple<EventInfo, Delegate>>();
                return;
            }

            Type genericPageType = viewBaseType.GetTypeInfo().BaseType;
            Type viewModelType = genericPageType.GenericTypeArguments[0];

            TypeInfo viewModelTypeInfo = viewModelType.GetTypeInfo();
            Dbg.Assert(typeof(IViewModel).GetTypeInfo().IsAssignableFrom(viewModelTypeInfo));

            if (navParameter != null)
            {
                if (viewModelTypeInfo.IsAssignableFrom(navParameter.GetType().GetTypeInfo()))
                {
                    this.contentViewModel = (IViewModel)navParameter;
                }
                else
                {
                    NavigationParameter parameter = navParameter as NavigationParameter;
                    Dbg.Assert(parameter != null);

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
            }
            else
            {
                this.contentViewModel = (IViewModel)this.Container.Resolve(viewModelType);
            }

            // Wire up any events on the ViewModel to conventionally named handles on the View
            Dbg.Assert(!this.autoMethodHandlers.ContainsKey(newContent));
            var autoHandlers = new List<Tuple<EventInfo, Delegate>>();

            IEnumerable<EventInfo> vmEvents = viewModelType.GetRuntimeEvents();
            foreach (EventInfo evt in vmEvents)
            {
                Type handlerType = evt.EventHandlerType;
                MethodInfo invokeMethod = handlerType.GetRuntimeMethods().First(method => method.Name == "Invoke");

                // By convention, auto-handlers will be named "EventNameHandler"
                string handlerName = $"{evt.Name}Handler";
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
                    autoHandlers.Add(new Tuple<EventInfo, Delegate>(evt, handlerDelegate));

                    Dbg.Trace($"Attached auto-EventHandler {handlerDelegate} for event {evt}");
                }
            }

            this.autoMethodHandlers[newContent] = autoHandlers;

            // Finally, attach the ViewModel to the new View
            newContent.DataContext = this.contentViewModel;

            Dbg.Trace("Successfully wired DataContext ViewModel to new RootFrame content!");
        }

        /// <summary>
        /// Tears down event handlers associated with a page when it is going away.
        /// </summary>
        /// <param name="previousContent">The content that is navigating into oblivion.</param>
        private void UnloadFrameContent(PassKeepPage previousContent)
        {
            Dbg.Assert(previousContent != null);

            // Abort any current load operation
            if (this.activeLoadingCts != null)
            {
                this.activeLoadingCts.Cancel();
            }

            // Tear down loading event handlers
            previousContent.StartedLoading -= ContentFrameStartedLoading;
            previousContent.DoneLoading -= ContentFrameDoneLoading;

            Dbg.Assert(this.autoMethodHandlers.ContainsKey(previousContent));
            var autoHandlers = this.autoMethodHandlers[previousContent];

            // Unregister any event handlers we set up automatically
            while (autoHandlers.Count > 0)
            {
                var autoHandler = autoHandlers[0];
                autoHandlers.RemoveAt(0);

                autoHandler.Item1.RemoveEventHandler(this.contentViewModel, autoHandler.Item2);
                Dbg.Trace($"Removed auto-EventHandler {autoHandler.Item2} for event {autoHandler.Item1.Name}");
            }

            this.autoMethodHandlers.Remove(previousContent);

            // If this Frame was hosting a page that hosted other pages, stop tracking that page's
            // content as it is being unloaded.
            INestingPage previousHostContent = previousContent as INestingPage;
            if (previousHostContent != null)
            {
                ForgetFrame(previousHostContent.ContentFrame);
            }
        }

        /// <summary>
        /// Latches onto the <paramref name="frame"/>'s navigation events to handle
        /// config-by-convention wiring.
        /// </summary>
        /// <param name="frame">The <see cref="Frame"/> to track.</param>
        private void TrackFrame(Frame frame)
        {
            Dbg.Assert(frame != null);

            frame.Navigating += this.TrackedFrame_Navigating;
            frame.Navigated += this.TrackedFrame_Navigated;
        }

        /// <summary>
        /// Unregisters event handles 
        /// </summary>
        /// <param name="frame"></param>
        private void ForgetFrame(Frame frame)
        {
            Dbg.Assert(frame != null);

            frame.Navigating -= this.TrackedFrame_Navigating;
            frame.Navigated -= this.TrackedFrame_Navigated;

            // Tear down any content of the frame
            PassKeepPage content = frame.Content as PassKeepPage;
            if (content != null)
            {
                UnloadFrameContent(content);
            }
        }

        #region Declaratively bound event handlers

        /// <summary>
        /// Invoked when the content Frame of the RootView is Navigating.
        /// </summary>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void TrackedFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Frame thisFrame = sender as Frame;
            Dbg.Assert(thisFrame != null);

            // Tear down any content of the frame
            PassKeepPage content = thisFrame.Content as PassKeepPage;
            if (content != null)
            {
                UnloadFrameContent(content);
            }
        }

        /// <summary>
        /// Invoked when the content Frame of the RootView is done navigating.
        /// </summary>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (this.splitViewNavigation)
            {

                this.contentFrame.BackStack.Clear();
                this.splitViewNavigation = false;
            }
            else
            { 
                SynchronizeNavigationListView();
            }

            TrackedFrame_Navigated(sender, e);
        }

        /// <summary>
        /// Invoked when a tracked content Frame is done navigating.
        /// </summary>
        /// <remarks>
        /// Hooks up the new content Page's IOC logic.
        /// </remarks>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void TrackedFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PassKeepPage newContent = e.Content as PassKeepPage;
            Dbg.Assert(newContent != null, "A content Frame should always navigate to a PassKeepPage");

            // Build up the new PassKeep Page
            HandleNewFrameContent(newContent, e.Parameter);
        }

        #endregion

        /// <summary>
        /// Updates the selected item of the navigation ListView without navigating.
        /// </summary>
        private void SynchronizeNavigationListView()
        {
            if (this.contentFrame.Content is DashboardView)
            {
                SetNavigationListViewSelection(this.dashItem);
            }
            else if (this.contentFrame.Content is DatabaseUnlockView)
            {
                SetNavigationListViewSelection(this.openItem);
            }
            else if (this.contentFrame.Content is DatabaseParentView)
            {
                SetNavigationListViewSelection(this.dbHomeItem);
            }
            else
            {
                SetNavigationListViewSelection(null);
            }
        }

        /// <summary>
        /// Helper to set the selected value of the SplitView nav list without invoking the event handler.
        /// </summary>
        /// <param name="selection">The value to forcibly select.</param>
        private void SetNavigationListViewSelection(object selection)
        {
            lock(this.splitViewSyncRoot)
            { 
                this.splitViewList.SelectionChanged -= SplitViewList_SelectionChanged;
                this.splitViewList.SelectedItem = selection;
                this.splitViewList.SelectionChanged += SplitViewList_SelectionChanged;
            }
        }

        /// <summary>
        /// Handles the expiration of a clipboard clear timer by clearing the clipboard.
        /// </summary>
        /// <param name="sender">The timer ViewModel.</param>
        /// <param name="e">Args for the expiration event.</param>
        private void ClipboardClearTimer_Complete(object sender, ClipboardTimerCompleteEventArgs e)
        {
            IClipboardClearTimerViewModel vm = sender as IClipboardClearTimerViewModel;
            Dbg.Assert(vm != null);

            // First validate that we should still be clearing the clipboard.
            // For example, a user may have disabled the option while the timer was in-progress.
            if (e.TimerType == ClipboardTimerType.UserName && !vm.UserNameClearEnabled)
            {
                return;
            }
            else if (e.TimerType == ClipboardTimerType.Password && !vm.PasswordClearEnabled)
            {
                return;
            }

            // Clear the clipboard, and if it fails (e.g., the app was out of focus), try to recover.
            try
            {
                Clipboard.Clear();
            }
            catch(Exception)
            {
                // No need to await this call.
#pragma warning disable CS4014
                // In the event of a failure, prompt the user to clear the clipboard manually.
                this.Dispatcher.RunAsync(
#pragma warning restore CS4014
                    CoreDispatcherPriority.Normal,
                    async () =>
                    {
                        MessageDialog dialog = new MessageDialog(GetString("ClipboardClearError"), GetString("ClearClipboardPrompt"))
                        {
                            Options = MessageDialogOptions.None
                        };

                        IUICommand clearCommand = new UICommand(GetString("ClearClipboardAction"));
                        IUICommand cancelCmd = new UICommand(GetString("Cancel"));

                        dialog.Commands.Add(clearCommand);
                        dialog.Commands.Add(cancelCmd);

                        dialog.DefaultCommandIndex = 0;
                        dialog.CancelCommandIndex = 1;

                        IUICommand chosenCmd = await dialog.ShowAsync();
                        if (chosenCmd == clearCommand)
                        {
                            Clipboard.Clear();
                        }
                    }
                );
            }
        }

        /// <summary>
        /// Invoked when the user manually opens or closed the SplitView panel.
        /// </summary>
        /// <param name="sender">The hamburger button.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void SplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = !this.mainSplitView.IsPaneOpen;
            Dbg.Trace($"SplitView.IsPaneOpen has been toggled to new state: {this.mainSplitView.IsPaneOpen}");
        }

        /// <summary>
        /// Invoked whenever the user selects a different SplitView option.
        /// </summary>
        /// <param name="sender">The ListView hosted in the SplitView panel.</param>
        /// <param name="e">EventARgs for the selection change.</param>
        private async void SplitViewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.contentFrame == null)
            {
                return;
            }

            Dbg.Assert(e.AddedItems.Count == 1);
            object selection = e.AddedItems[0];
            object deselection = e.RemovedItems.Count == 1 ? e.RemovedItems[0] : null;

            // Helper function for reverting the SelectedItem to the previous value,
            // for buttons that aren't "real" navigates.
            Action abortSelection = () =>
            {
                Dbg.Assert(deselection != null);
                SetNavigationListViewSelection(deselection);
            };

            if (selection == this.dashItem && deselection != this.dashItem)
            {
                Dbg.Trace("Dashboard selected in SplitView.");
                this.splitViewNavigation = true;
                this.contentFrame.Navigate(typeof(DashboardView));
            }
            else if (selection == this.openItem)
            {
                Dbg.Trace("Open selected in SplitView.");
                await PickFile(
                    /* gotFile */ file =>
                    {
                        OpenFile(file);
                    },
                    /* cancelled */
                    abortSelection
                );
            }
            else if (selection == this.dbHomeItem)
            {
                Dbg.Trace("Database Home selected in SplitView.");
                this.splitViewNavigation = true;
                Dbg.Assert(this.ViewModel.DecryptedDatabase != null, "This button should not be accessible if there is not decrypted database");
                this.contentFrame.Navigate(typeof(DatabaseParentView), this.ViewModel.DecryptedDatabase);
            }
            else if (selection == this.passwordItem)
            {
                Dbg.Trace("Password Generator selected in SplitView.");
                abortSelection();
            }
            else if (selection == this.helpItem)
            {
                Dbg.Trace("Help selected in SplitView.");
                abortSelection();

                OpenFlyout(new HelpFlyout());
            }
            else if (selection == this.settingsItem)
            {
                Dbg.Trace("Settings selected in SplitView.");
                abortSelection();

                AppSettingsFlyout flyout = new AppSettingsFlyout
                {
                    ViewModel = Container.Resolve<IAppSettingsViewModel>()
                };
                OpenFlyout(flyout);
            }
        }

        /// <summary>
        /// Helper for dealing with SettingsFlyouts.
        /// </summary>
        /// <param name="flyout"></param>
        private void OpenFlyout(SettingsFlyout flyout)
        {
            // Default BackClick behavior brings up the legacy Settings Pane, which is undesirable.
            flyout.BackClick += (s, e) =>
            {
                flyout.Hide();
                e.Handled = true;
            };

            flyout.Show();
        }
    }
}
