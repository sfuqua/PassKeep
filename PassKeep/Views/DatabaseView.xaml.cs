using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PassKeep.Controls;
using PassKeep.ViewModels;
using Windows.ApplicationModel.Search;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PassKeep.Common;
using PassKeep.Lib.EventArgClasses;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace PassKeep.Views
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {
        private const int SnappedEntryPanelHeight = 320;
        public DatabaseArrangementViewModel ArrangementViewModel
        {
            get;
            set;
        }

        public override bool IsProtected
        {
            get { return true; }
        }

        public override Task<bool> Lock()
        {
            //Navigator.ReplacePage(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, ViewModel.File));
            throw new NotImplementedException();
            //return Task.Run(() => true);
        }

        public override bool SearchOnKeypress
        {
            get
            {
                return true;
            }
        }

        private Storyboard activeEntryColumnAnimation;
        private Storyboard snappedEntryPanelAnimation;

        private bool ignoreGridViewChanges = false;
        private bool ignoreListViewChanges = false;

        private Button createButton;
        public DatabaseView()
        {
            this.InitializeComponent();

            createButton = new Button();
            Style btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["AddAppBarButtonStyle"];
            btnStyle.Setters.Add(
                new Setter(AutomationProperties.AutomationIdProperty, "CreateAppBarButton")
            );
            btnStyle.Setters.Add(
                new Setter(AutomationProperties.NameProperty, "Create...")
            );
            createButton.Style = btnStyle;
            createButton.Click += (s, e) =>
                {
                    Popup p = new Popup
                    {
                        IsLightDismissEnabled = true
                    };
                    StackPanel panel = new StackPanel 
                    {
                        Orientation = Orientation.Vertical,
                        Background = BottomAppBar.Background,
                        Height = 68, Width = 106
                    };

                    Button newEntryBtn = new Button
                    {
                        Content = "New Entry",
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0),
                        Width = 106
                    };
                    newEntryBtn.Click += (s_i, e_i) =>
                    {
                        createNewEntry();
                        p.IsOpen = false;
                    };
                    panel.Children.Add(newEntryBtn);

                    Button newGroupBtn = new Button
                    {
                        Content = "New Group",
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0),
                        Width = 106
                    };
                    newGroupBtn.Click += (s_i, e_i) =>
                    {
                        createNewGroup();
                        p.IsOpen = false;
                    };
                    panel.Children.Add(newGroupBtn);

                    var windowBounds = Window.Current.CoreWindow.Bounds;
                    p.HorizontalOffset = (windowBounds.Right - windowBounds.Left) - 
                        panel.Width - 4;
                    p.VerticalOffset = (windowBounds.Bottom - windowBounds.Top) -
                        BottomAppBar.ActualHeight - panel.Height - 4;

                    p.Child = panel;
                    p.IsOpen = true;
                };

            editButton = new Button();
            editButton.Click += (s, e_i) =>
            {
                editSelection();
            };
            btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["EditAppBarButtonStyle"];
            editButton.Style = btnStyle;

            deleteButton = new Button();
            btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["DeleteAppBarButtonStyle"];
            btnStyle.Setters.Add(new Setter(Button.ContentProperty, "\uE107"));
            deleteButton.Style = btnStyle;
            deleteButton.Click += (s, e_i) => { deleteSelection(); };

            SizeChanged += handleResize;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                fullscreenGroupGridView.SetBinding(
                    GridView.ItemsSourceProperty, new Binding { Source = databaseSource }
                );
                itemListViewSnapped.SetBinding(
                    ListView.ItemsSourceProperty, new Binding { Source = databaseSource }
                );
            }
        }

        /// <summary>
        /// Called whenever the window resizes (for example, thanks to snapped view or a shift to portrait).
        /// Responsible for adjusting the width of MainContentBorder, which contains the primary GridView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void handleResize(object sender, SizeChangedEventArgs e)
        {
            bool hasActiveEntry = (ViewModel != null && ViewModel.BreadcrumbViewModel.ActiveLeaf != null);
            
            // Compute EntryColumn
            if (ApplicationView.Value != ApplicationViewState.Snapped && hasActiveEntry)
            {
                Debug.WriteLine("EntryColumn should be visible; setting MainContentBorder.Width to " + (Window.Current.Bounds.Width - EntryColumn.Width.Value));
                MainContentBorder.Width = Window.Current.Bounds.Width - EntryColumn.Width.Value;
            }
            else
            {
                Debug.WriteLine("EntryColumn should not be visible; setting MainContentBorder.Width to " + Window.Current.Bounds.Width);
                MainContentBorder.Width = Window.Current.Bounds.Width;
            }

            // Compute SnappedEntryPanel
            if (ApplicationView.Value == ApplicationViewState.Snapped && hasActiveEntry)
            {
                SnappedEntryPanel.Height = SnappedEntryPanelHeight;
                SnappedEntryPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SnappedEntryPanel.Height = 0;
                SnappedEntryPanel.Visibility = Visibility.Collapsed;
            }

            // Make sure the snapped selection is visible
            if (ApplicationView.Value == ApplicationViewState.Snapped && itemListViewSnapped.SelectedItem != null)
            {
                itemListViewSnapped.LayoutUpdated += scrollSnappedItemIntoView;
                UpdateLayout();
            }
        }

        private async void scrollSnappedItemIntoView(object sender, object eventArgs)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { itemListViewSnapped.ScrollIntoView(itemListViewSnapped.SelectedItem); });
            itemListViewSnapped.LayoutUpdated -= scrollSnappedItemIntoView;
        }

        #region AppBar buttons

        public override void SetupDefaultAppBarButtons()
        {
            CustomAppBarButtons.Add(createButton);
            ListViewBase listView = (ApplicationView.Value != ApplicationViewState.Snapped ? (ListViewBase)fullscreenGroupGridView : itemListViewSnapped);
            if (listView.SelectedItems.Count > 0)
            {
                CustomAppBarButtons.Insert(0, deleteButton);
            }
            if (listView.SelectedItems.Count == 1)
            {
                CustomAppBarButtons.Insert(0, editButton);
            }
        }

        public override void SetupSnappedAppBarButtons()
        {
            LeftCommands.Visibility = Visibility.Collapsed;
            SetupDefaultAppBarButtons();
        }

        #endregion

        public override Task<bool> HandleHotKey(VirtualKey key)
        {
            switch(key)
            {
                case VirtualKey.I:
                    createNewEntry();
                    return Task.Run(() => true);
                case VirtualKey.G:
                    createNewGroup();
                    return Task.Run(() => true);
                case VirtualKey.D:
                    editSelection();
                    return Task.Run(() => true);
                case VirtualKey.C:
                    copySelection();
                    return Task.Run(() => true);
                case VirtualKey.X:
                    cutSelection();
                    return Task.Run(() => true);
                case VirtualKey.V:
                    pasteSelection();
                    return Task.Run(() => true);
                default:
                    return Task.Run(() => false);
            }
        }

        public override Task<bool> HandleDelete()
        {
            deleteSelection();
            return Task.Run(() => true); ;
        }

        private void createNewEntry()
        {
            IKeePassGroup currentGroup = ViewModel.BreadcrumbViewModel.ActiveGroup;
            IKeePassEntry newEntry = new KdbxEntry(currentGroup, ViewModel.GetRng(), ViewModel.GetDbMetadata());
            Frame.Navigate(typeof(EntryDetailsView), ViewModel.GetEntryDetailViewModel(newEntry, true));
        }

        private void createNewGroup()
        {
            IKeePassGroup currentGroup = ViewModel.BreadcrumbViewModel.ActiveGroup;
            IKeePassGroup newGroup = new KdbxGroup(currentGroup);
            Frame.Navigate(typeof(GroupDetailsView), ViewModel.GetGroupDetailViewModel(newGroup, true));
        }

        private void editSelection()
        {
            ListViewBase listView = (ApplicationView.Value == ApplicationViewState.Snapped ?
                (ListViewBase)itemListViewSnapped : (ListViewBase)fullscreenGroupGridView);

            if (listView.SelectedItems.Count > 1)
            {
                return;
            }

            object clicked = listView.SelectedItem;
            if (clicked == null)
            {
                clicked = ViewModel.BreadcrumbViewModel.ActiveLeaf;
            }
            if (clicked == null)
            {
                clicked = ViewModel.BreadcrumbViewModel.ActiveGroup;
            }
            if (clicked == null)
            {
                return;
            }

            if (clicked is KdbxEntry)
            {
                KdbxEntry entry = (KdbxEntry)clicked;
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    ViewModel.GetEntryDetailViewModel(entry, true)
                );
            }
            else if (clicked is KdbxGroup)
            {
                KdbxGroup group = (KdbxGroup)clicked;
                Frame.Navigate(
                    typeof(GroupDetailsView),
                    ViewModel.GetGroupDetailViewModel(group, true)
                );
            }
        }

        private async void deleteSelection(bool prompt = true)
        {
            ListViewBase listView = (ApplicationView.Value == ApplicationViewState.Snapped ?
                (ListViewBase)itemListViewSnapped : (ListViewBase)fullscreenGroupGridView);

            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            if (prompt)
            {
                var dialog = new MessageDialog("Do you wish to permanently delete the selected items?", "Are you sure?");
                IUICommand yesCmd = new UICommand("Yes");
                IUICommand noCmd = new UICommand("No");
                dialog.Commands.Add(yesCmd);
                dialog.Commands.Add(noCmd);
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var chosenCmd = await dialog.ShowAsync();
                if (chosenCmd == noCmd)
                {
                    return;
                }
            }

            IKeePassGroup currentGroup = ViewModel.BreadcrumbViewModel.ActiveGroup;
            IList<Tuple<int, IKeePassGroup>> removedGroups = new List<Tuple<int, IKeePassGroup>>();
            IList<Tuple<int, IKeePassEntry>> removedEntries = new List<Tuple<int, IKeePassEntry>>();

            List<IKeePassNode> selectedNodes = listView.SelectedItems.Cast<IKeePassNode>().ToList();
            Debug.WriteLine("Deleting {0} selected items.", selectedNodes.Count);
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                IKeePassNode selection = selectedNodes[i];
                if (selection is IKeePassGroup)
                {
                    IKeePassGroup thisGroup = (IKeePassGroup)selection;
                    int index = currentGroup.Groups.IndexOf(thisGroup);
                    removedGroups.Add(new Tuple<int, IKeePassGroup>(index, thisGroup));
                    currentGroup.Groups.RemoveAt(index);
                }
                else if (selection is IKeePassEntry)
                {
                    IKeePassEntry thisEntry = (KdbxEntry)selection;
                    int index = currentGroup.Entries.IndexOf(thisEntry);
                    removedEntries.Add(new Tuple<int, IKeePassEntry>(index, thisEntry));
                    currentGroup.Entries.RemoveAt(index);
                }
            }

            if (!(await ViewModel.Commit()))
            {
                // Restore
                foreach (var tup in removedGroups)
                {
                    if (tup.Item1 == currentGroup.Groups.Count)
                    {
                        currentGroup.Groups.Add(tup.Item2);
                    }
                    else
                    {
                        currentGroup.Groups.Insert(tup.Item1, tup.Item2);
                    }
                }

                foreach (var tup in removedEntries)
                {
                    if (tup.Item1 == currentGroup.Entries.Count)
                    {
                        currentGroup.Entries.Add(tup.Item2);
                    }
                    else
                    {
                        currentGroup.Entries.Insert(tup.Item1, tup.Item2);
                    }
                }
            }
            else
            {
                listView.SelectedItem = null;
                foreach (var tup in removedGroups)
                {
                    ViewModel.BreadcrumbViewModel.ActiveGroup.Children.Remove(tup.Item2);
                }

                foreach (var tup in removedEntries)
                {
                    ViewModel.BreadcrumbViewModel.ActiveGroup.Children.Remove(tup.Item2);
                }
            }
        }

        private void cutSelection()
        {

        }

        private void copySelection()
        {

        }

        private void pasteSelection()
        {

        }

        private void setupAnimation(ref Storyboard storyboard, EventHandler<object> timelineCompleted, double from, double to, UIElement target, string propertyPath, double timeInMilliseconds = 400)
        {
            Debug.WriteLine("Animating {0} from {1} to {2}.", propertyPath, from, to);
            Debug.Assert(from >= 0);
            Debug.Assert(to >= 0);
            Debug.Assert(timeInMilliseconds >= 0);
            if (from < 0 || to < 0 || timeInMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (propertyPath != "Width" && propertyPath != "Height")
            {
                throw new ArgumentException();
            }

            if (storyboard != null)
            {
                storyboard.SkipToFill();
            }

            if (to > 0)
            {
                target.Visibility = Visibility.Visible;
            }

            Storyboard newStoryboard = new Storyboard();
            DoubleAnimation newAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(timeInMilliseconds)),
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(newAnimation, target);
            Storyboard.SetTargetProperty(newAnimation, propertyPath);
            newStoryboard.Children.Add(newAnimation);
            newStoryboard.Completed += timelineCompleted;
            newStoryboard.Completed += (s_i, e_i) =>
            {
                if (to == 0)
                {
                    target.Visibility = Visibility.Collapsed;
                }
            };
            newStoryboard.Begin();

            storyboard = newStoryboard;
        }

        private void setupEntryColumnAnimation(double from, double to)
        {
            setupAnimation(ref activeEntryColumnAnimation, (s, e) => { activeEntryColumnAnimation = null; }, from, to, MainContentBorder, "Width");
        }

        private void setupSnappedEntryPanelAnimation(double from, double to, bool immediate = false)
        {
            EventHandler<object> handler = (s, e) =>
            {
                snappedEntryPanelAnimation = null;
                if (itemListViewSnapped.SelectedItem != null)
                {
                    itemListViewSnapped.ScrollIntoView(itemListViewSnapped.SelectedItem);
                }
            };

            if (!immediate)
            {
                setupAnimation(ref snappedEntryPanelAnimation, handler, from, to, SnappedEntryPanel, "Height");
            }
            else
            {
                setupAnimation(ref snappedEntryPanelAnimation, handler, from, to, SnappedEntryPanel, "Height", 0);
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        protected override void navHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            base.navHelper_SaveState(sender, e);
            e.PageState["LayoutState"] = ApplicationView.Value.ToString();
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                var listView = FindVisualChild<ScrollViewer>(itemListViewSnapped);
                e.PageState["ListScrollerPosition"] = (listView != null ? listView.VerticalOffset : 0);
            }
            else
            {
                var gridView = FindVisualChild<ScrollViewer>(fullscreenGroupGridView);
                e.PageState["GridScrollerPosition"] = (gridView != null ? gridView.HorizontalOffset : 0);
            }
        }

        private void StartedWriteHandler(object sender, CancelableEventArgs e)
        {
            RaiseStartedLoading("Saving...", e.Cancel);
        }

        private void DoneWriteHandler(object sender, EventArgs e)
        {
            RaiseDoneLoading();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);

            if (e.PageState != null)
            {
                object layoutState = e.PageState["LayoutState"];
                ApplicationViewState state;
                if (layoutState != null && Enum.TryParse(layoutState.ToString(), out state))
                {
                    if (state == ApplicationView.Value)
                    {
                        if (state == ApplicationViewState.Snapped)
                        {
                            object scrollPos = e.PageState["ListScrollerPosition"];
                            if (scrollPos != null)
                            {

                                FindVisualChild<ScrollViewer>(itemListViewSnapped).ScrollToVerticalOffset((double)scrollPos);
                            }
                        }
                        else
                        {
                            object scrollPos = e.PageState["GridScrollerPosition"];
                            if (scrollPos != null)
                            {
                                FindVisualChild<ScrollViewer>(fullscreenGroupGridView).ScrollToHorizontalOffset((double)scrollPos);
                            }
                        }
                    }
                }
            }
            
            ViewModel.ActiveEntryChanged += ActiveEntryChangedHandler;
            ViewModel.DetailsRequested += ShowDetails;
            ViewModel.StartedWrite += StartedWriteHandler;
            ViewModel.DoneWrite += DoneWriteHandler;

            EntryPreviewClipboardViewModel sharedClipboardViewModel = new EntryPreviewClipboardViewModel(ViewModel.Settings);
            FullscreenEntryControl.ClipboardViewModel = sharedClipboardViewModel;
            SnappedEntryControl.ClipboardViewModel = sharedClipboardViewModel;

            if (await ViewModel.BuildTree())
            {
                Debug.Assert(false);
            }

            ViewModel.BreadcrumbViewModel.LeavesChanged += activeGroupChildrenChanged;
            ArrangementViewModel = new DatabaseArrangementViewModel(ViewModel.Settings);
            organizeChildren();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.ActiveEntryChanged -= ActiveEntryChangedHandler;
            ViewModel.DetailsRequested -= ShowDetails;
            ViewModel.StartedWrite -= StartedWriteHandler;
            ViewModel.DoneWrite -= DoneWriteHandler;
            ViewModel.BreadcrumbViewModel.LeavesChanged -= activeGroupChildrenChanged;

            FullscreenEntryControl.ClipboardViewModel.Cleanup();
            SnappedEntryControl.ClipboardViewModel.Cleanup();

            base.OnNavigatedFrom(e);
        }

        private void organizeChildren()
        {
            ArrangementViewModel.ArrangeChildren(ViewModel.BreadcrumbViewModel.ActiveGroup, ref databaseSource);
            //fullscreenGroupGridView.ItemsSource = databaseSource.Source;
            //itemListViewSnapped.ItemsSource = databaseSource.Source;
        }

        private void activeGroupChildrenChanged(object sender, EventArgs e)
        {
            organizeChildren();
        }

        private void ActiveEntryChangedHandler(object sender, ActiveEntryChangedEventArgs e)
        {
            // Are we selecting something (from nothing)?
            if (e.NewEntry != null && e.OldEntry == null)
            {
                if (ApplicationView.Value != ApplicationViewState.Snapped)
                {
                    setupEntryColumnAnimation(Window.Current.Bounds.Width, Window.Current.Bounds.Width - EntryColumn.Width.Value);
                }
                else
                {
                    setupSnappedEntryPanelAnimation(0, SnappedEntryPanelHeight);
                }
            }
            // Do we no longer have a single Active Entry?
            else if (e.OldEntry != null && e.NewEntry == null)
            {
                hideActiveEntryPanel();
            }
        }

        protected override void TryGoBack()
        {
            if (!ViewModel.GoUp())
            {
                base.TryGoBack();
            }
        }

        private bool itemClicked = false;
        private void itemClick(object sender, ItemClickEventArgs e)
        {
            ListViewBase listView = (ListViewBase)sender;
            ListViewBase otherList = (listView == fullscreenGroupGridView ? (ListViewBase)itemListViewSnapped : (ListViewBase)fullscreenGroupGridView);

            itemClicked = true;
            ViewModel.Select(e.ClickedItem as IKeePassNode);

            if (e.ClickedItem is IKeePassEntry)
            {
                listView.SelectedItem = e.ClickedItem;
            }
            else
            {
                listView.SelectedItem = null;
            }
        }

        private void breadcrumbClick(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Debug.Assert(clickedGroup != null);
            Debug.Assert(ViewModel != null);

            ViewModel.BreadcrumbViewModel.SetGroup(clickedGroup);
        }

        /// <summary>
        /// Animates the active entry panel out of the way depending on viewport state.
        /// </summary>
        private void hideActiveEntryPanel()
        {
            Storyboard activeStoryboard;
            ListViewBase inactiveList;
            if (ApplicationView.Value != ApplicationViewState.Snapped)
            {
                setupEntryColumnAnimation(Window.Current.Bounds.Width - EntryColumn.Width.Value, Window.Current.Bounds.Width);
                activeStoryboard = activeEntryColumnAnimation;
                inactiveList = itemListViewSnapped;
            }
            else
            {
                setupSnappedEntryPanelAnimation(SnappedEntryPanelHeight, 0);
                activeStoryboard = snappedEntryPanelAnimation;
                inactiveList = fullscreenGroupGridView;
            }
        }

        private void btnExitEntry_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BreadcrumbViewModel.Prune();
        }

        /// <summary>
        /// Exists to be called by the SelectionChanged event handler - synchronizes the selected items
        /// of the inactive ListViewBase with those of the active one.
        /// </summary>
        /// <param name="changedList"></param>
        /// <param name="eventArgs"></param>
        private async void synchronizeSelectionChanges(ListViewBase changedList, SelectionChangedEventArgs eventArgs)
        {
            Debug.WriteLine(
                "Before synchronizing selections, GridView has {0} selections and ListView has {1}.",
                fullscreenGroupGridView.SelectedItems.Count,
                itemListViewSnapped.SelectedItems.Count
            );

            // Determine which list we're dealing with 
            ListViewBase otherList = (changedList == fullscreenGroupGridView ? (ListViewBase)itemListViewSnapped : (ListViewBase)fullscreenGroupGridView);
            Debug.WriteLine("We're synchronizing changes for the {0}.", otherList == fullscreenGroupGridView ? "GridView" : "ListView");

            if (changedList.SelectedItems.Count == otherList.SelectedItems.Count)
            {
                bool allMatched = true;
                for (int i = 0; i < changedList.SelectedItems.Count; i++ )
                {
                    if (changedList.SelectedItems[i] != otherList.SelectedItems[i])
                    {
                        allMatched = false;
                        break;
                    }
                }

                if (allMatched)
                {
                    Debug.WriteLine("No sync needed.");
                    return;
                }
            }

            // Temporarily detach the event handler while we manipulate the selection
            if (otherList == fullscreenGroupGridView)
            {
                ignoreGridViewChanges = true;
            }
            else
            {
                ignoreListViewChanges = true;
            }

            // Add and remove added/removed items
            foreach (object removed in eventArgs.RemovedItems)
            {
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Debug.WriteLine("Removing a synchronized item...");
                        otherList.SelectedItems.Remove(removed);
                        Debug.WriteLine("...removal successful.");
                    }
                );
            }
            foreach (object added in eventArgs.AddedItems)
            {
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Debug.WriteLine("Adding a synchronized item...");
                        otherList.SelectedItems.Add(added);
                        Debug.WriteLine("...addition successfully.");
                    }
                );
            }

            // Re-attach the event handler
            if (otherList == fullscreenGroupGridView)
            {
                ignoreGridViewChanges = false;
            }
            else
            {
                ignoreListViewChanges = false;
            }

            Debug.WriteLine(
                "After synchronizing selections, GridView has {0} selections and ListView has {1}.",
                fullscreenGroupGridView.SelectedItems.Count,
                itemListViewSnapped.SelectedItems.Count
            );
        }

        private Button deleteButton;
        private Button editButton;
        private void entriesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == fullscreenGroupGridView && ignoreGridViewChanges)
            {
                Debug.WriteLine("Ignored a selection change for the GridView.");
                return;
            }

            if (sender == itemListViewSnapped && ignoreListViewChanges)
            {
                Debug.WriteLine("Ignored a selection change for the ListView.");
                return;
            }

            Debug.WriteLine("Selection changed for the {0}.", sender == fullscreenGroupGridView ? "GridView" : "ListView");
            Debug.WriteLine("\tCurrently selecting {0} items.", ((ListViewBase)sender).SelectedItems.Count);
            Debug.WriteLine("\t{0} were added, {1} were removed.", e.AddedItems.Count, e.RemovedItems.Count);

            // Load new app bar buttons as needed (delete, edit, etc)
            RefreshAppBarButtons();

            ListViewBase listView = (ListViewBase)sender;
            synchronizeSelectionChanges(listView, e);

            // If we are at 0 selected items or more than 1, remove the "ActiveLeaf"  
            if (listView.SelectedItems.Count != 1)
            {
                ViewModel.BreadcrumbViewModel.Prune();
            }

            // If we've deselected everything, make sure the app bar is hidden.
            if (listView.SelectedItems.Count == 0)
            {
                BottomAppBar.IsSticky = false;
                BottomAppBar.IsOpen = false;
            }
            else
            {
                // If we clicked on something, don't show the app bar.
                if (itemClicked)
                {
                    itemClicked = false;
                    return;
                }

                BottomAppBar.IsSticky = true;
                BottomAppBar.IsOpen = true;
            }
        }

        private void itemsLoaded(object sender, RoutedEventArgs e)
        {
            //((ListViewBase)sender).SelectionMode = ListViewSelectionMode.Multiple;
            //((ListViewBase)sender).IsSwipeEnabled = true;

            ((ListViewBase)sender).CanReorderItems = false;
            ((ListViewBase)sender).CanDragItems = false; // TODO
            ((ListViewBase)sender).AllowDrop = false; // TODO

            if (ViewModel.BreadcrumbViewModel.ActiveLeaf != null)
            {
                itemClicked = true;
                if (ApplicationView.Value == ApplicationViewState.Snapped)
                {
                    itemListViewSnapped.SelectedItem = ViewModel.BreadcrumbViewModel.ActiveLeaf;
                }
                else
                {
                    fullscreenGroupGridView.SelectedItem = ViewModel.BreadcrumbViewModel.ActiveLeaf;
                }
            }
        }

        public void ShowDetails(object sender, EventArgs e)
        {
            // GetEntryDetailViewModel no longer exists
            Frame.Navigate(typeof(EntryDetailsView), ViewModel.GetEntryDetailViewModel((KdbxEntry)ViewModel.BreadcrumbViewModel.ActiveLeaf));
        }

        private void SortMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            organizeChildren();
        }
    }
}
