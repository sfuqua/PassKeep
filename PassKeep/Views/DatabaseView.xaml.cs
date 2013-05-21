using System;
using System.Collections.Generic;
using System.Diagnostics;
using PassKeep.Common;
using PassKeep.Controls;
using PassKeep.ViewModels;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using PassKeep.Models;
using System.Collections.ObjectModel;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace PassKeep.Views
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {
        public override bool IsProtected
        {
            get { return true; }
        }

        public override void Lock()
        {
            Navigator.ReplacePage(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, ViewModel.File));
        }

        //private EntryPanel entryPanel;
        private Storyboard activeStoryboard;

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

            SizeChanged += (s, e) =>
            {
                if (ApplicationView.Value == ApplicationViewState.Snapped || (ViewModel != null && ViewModel.ActiveEntry == null))
                {
                    MainContentBorder.Width = ActualWidth;
                }
                else
                {
                    MainContentBorder.Width = ActualWidth - EntryColumn.Width.Value;
                }
            };
        }

        #region AppBar buttons

        public override void SetupDefaultAppBarButtons()
        {
            CustomAppBarButtons.Add(createButton);
            ListViewBase listView = (ApplicationView.Value != ApplicationViewState.Snapped ? (ListViewBase)itemGridView : itemListViewSnapped);
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

        public override void HandleHotKey(VirtualKey key)
        {
            switch(key)
            {
                case VirtualKey.I:
                    createNewEntry();
                    break;
                case VirtualKey.G:
                    createNewGroup();
                    break;
                case VirtualKey.D:
                    editSelection();
                    break;
            }
        }

        public override void HandleDelete()
        {
            deleteSelection();
        }

        private void createNewEntry()
        {
            KdbxGroup currentGroup = ViewModel.Breadcrumbs[ViewModel.Breadcrumbs.Count - 1];
            KdbxEntry newEntry = new KdbxEntry(currentGroup, ViewModel.GetRng(), ViewModel.GetDbMetadata());
            Navigator.Navigate(typeof(EntryDetailsView), ViewModel.GetEntryDetailViewModel(newEntry, true));
        }

        private void createNewGroup()
        {
            KdbxGroup currentGroup = ViewModel.Breadcrumbs[ViewModel.Breadcrumbs.Count - 1];
            KdbxGroup newGroup = new KdbxGroup(currentGroup);
            Navigator.Navigate(typeof(GroupDetailsView), ViewModel.GetGroupDetailViewModel(newGroup, true));
        }

        private void editSelection()
        {
            ListViewBase listView = (ApplicationView.Value == ApplicationViewState.Snapped ?
                (ListViewBase)itemListViewSnapped : (ListViewBase)itemGridView);

            if (listView.SelectedItems.Count > 1)
            {
                return;
            }

            object clicked = listView.SelectedItem;
            if (clicked == null)
            {
                clicked = ViewModel.ActiveEntry;
            }
            if (clicked == null && ViewModel.Breadcrumbs.Count > 1)
            {
                clicked = ViewModel.Breadcrumbs[ViewModel.Breadcrumbs.Count - 1];
            }
            if (clicked == null)
            {
                return;
            }

            if (clicked is KdbxEntry)
            {
                KdbxEntry entry = (KdbxEntry)clicked;
                Navigator.Navigate(
                    typeof(EntryDetailsView),
                    ViewModel.GetEntryDetailViewModel(entry, true)
                );
            }
            else if (clicked is KdbxGroup)
            {
                KdbxGroup group = (KdbxGroup)clicked;
                Navigator.Navigate(
                    typeof(GroupDetailsView),
                    ViewModel.GetGroupDetailViewModel(group, true)
                );
            }
        }

        private async void deleteSelection()
        {
            ListViewBase listView = (ApplicationView.Value == ApplicationViewState.Snapped ?
                (ListViewBase)itemListViewSnapped : (ListViewBase)itemGridView);

            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

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

            KdbxGroup currentGroup = ViewModel.Breadcrumbs[ViewModel.Breadcrumbs.Count - 1];
            IList<Tuple<int, KdbxGroup>> removedGroups = new List<Tuple<int, KdbxGroup>>();
            IList<Tuple<int, KdbxEntry>> removedEntries = new List<Tuple<int, KdbxEntry>>();
            for (int i = 0; i < listView.SelectedItems.Count; i++)
            {
                object selection = listView.SelectedItems[i];
                if (selection is KdbxGroup)
                {
                    KdbxGroup thisGroup = (KdbxGroup)selection;
                    int index = currentGroup.Groups.IndexOf(thisGroup);
                    removedGroups.Add(new Tuple<int,KdbxGroup>(index, thisGroup));
                    currentGroup.Groups.RemoveAt(index);
                }
                else if (selection is KdbxEntry)
                {
                    KdbxEntry thisEntry = (KdbxEntry)selection;
                    int index = currentGroup.Entries.IndexOf(thisEntry);
                    removedEntries.Add(new Tuple<int, KdbxEntry>(index, thisEntry));
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
                    ViewModel.Items.Remove(tup.Item2);
                }

                foreach (var tup in removedEntries)
                {
                    ViewModel.Entries.Remove(tup.Item2);
                    ViewModel.Items.Remove(tup.Item2);
                }
            }
        }

        private void setupAnimation(double from, double to)
        {
            if (activeStoryboard != null)
            {
                activeStoryboard.SkipToFill();
            }

            Storyboard storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(animation, MainContentBorder);
            Storyboard.SetTargetProperty(animation, "Width");
            storyboard.Children.Add(animation);
            storyboard.Begin();

            activeStoryboard = storyboard;
            storyboard.Completed += (s_i, e_i) =>
            {
                activeStoryboard = null;
            };
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

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            base.SaveState(pageState);
            pageState["LayoutState"] = ApplicationView.Value.ToString();
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                var listView = FindVisualChild<ScrollViewer>(itemListViewSnapped);
                pageState["ListScrollerPosition"] = (listView != null ? listView.VerticalOffset : 0);
            }
            else
            {
                var gridView = FindVisualChild<ScrollViewer>(itemGridView);
                pageState["GridScrollerPosition"] = (gridView != null ? gridView.HorizontalOffset : 0);
            }
        }

        private void StartedWriteHandler(object sender, CancelableEventArgs e)
        {
            onStartedLoading("Saving...", e.Cancel);
        }

        private void DoneWriteHandler(object sender, EventArgs e)
        {
            onDoneLoading();
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            base.LoadState(navigationParameter, pageState);

            if (pageState != null)
            {
                object layoutState = pageState["LayoutState"];
                ApplicationViewState state;
                if (layoutState != null && Enum.TryParse(layoutState.ToString(), out state))
                {
                    if (state == ApplicationView.Value)
                    {
                        if (state == ApplicationViewState.Snapped)
                        {
                            object scrollPos = pageState["ListScrollerPosition"];
                            if (scrollPos != null)
                            {

                                FindVisualChild<ScrollViewer>(itemListViewSnapped).ScrollToVerticalOffset((double)scrollPos);
                            }
                        }
                        else
                        {
                            object scrollPos = pageState["GridScrollerPosition"];
                            if (scrollPos != null)
                            {
                                FindVisualChild<ScrollViewer>(itemGridView).ScrollToHorizontalOffset((double)scrollPos);
                            }
                        }
                    }
                }
            }
            
            ViewModel.DirectoryLevelReset += DirectoryLevelResetHandler;
            ViewModel.DirectoryLevelIncreased += DirectoryLevelIncreaseHandler;
            ViewModel.ActiveEntryChanged += ActiveEntryChangedHandler;
            ViewModel.DetailsRequested += ShowDetails;
            ViewModel.StartedWrite += StartedWriteHandler;
            ViewModel.DoneWrite += DoneWriteHandler;

            if (!(await ViewModel.BuildTree()))
            {
                //ViewModel.Synchronize();
            }

            if (ViewModel.ActiveEntry != null)
            {
                itemClicked = true;
                if (ApplicationView.Value == ApplicationViewState.Snapped)
                {
                    itemListViewSnapped.SelectedItem = ViewModel.ActiveEntry;
                }
                else
                {
                    itemGridView.SelectedItem = ViewModel.ActiveEntry;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.DirectoryLevelReset -= DirectoryLevelResetHandler;
            ViewModel.DirectoryLevelIncreased -= DirectoryLevelIncreaseHandler;
            ViewModel.ActiveEntryChanged -= ActiveEntryChangedHandler;
            ViewModel.DetailsRequested -= ShowDetails;
            ViewModel.StartedWrite -= StartedWriteHandler;
            ViewModel.DoneWrite -= DoneWriteHandler;

            base.OnNavigatedFrom(e);
        }

        private void DirectoryLevelResetHandler(object sender, EventArgs e)
        {
            Navigator.BackstackOverride = false;
        }

        private void DirectoryLevelIncreaseHandler(object sender, EventArgs e)
        {
            Navigator.BackstackOverride = true;
        }

        private void ActiveEntryChangedHandler(object sender, ActiveEntryChangedEventArgs e)
        {
            if (ApplicationView.Value != ApplicationViewState.Snapped)
            {
                if (e.NewEntry != null && e.OldEntry == null)
                {
                    setupAnimation(ActualWidth, ActualWidth - EntryColumn.Width.Value);
                }
            }
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.GoUp())
            {
                Navigator.BackstackOverride = false;
                base.GoBack(sender, e);
            }
        }

        private bool itemClicked = false;
        private void itemClick(object sender, ItemClickEventArgs e)
        {
            ListViewBase listView = (ListViewBase)sender;

            itemClicked = true;
            ViewModel.Select(e.ClickedItem);

            if (e.ClickedItem is KdbxEntry)
            {
                listView.SelectedItem = e.ClickedItem;
            }
            else
            {
                listView.SelectedItem = null;
            }
        }

        private void breadcrumb_ItemClick(object sender, ItemClickEventArgs e)
        {
            int index = breadcrumb.Items.IndexOf(e.ClickedItem);
            ViewModel.Navigate(index);
        }

        private void btnExitEntry_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value != ApplicationViewState.Snapped)
            {
                setupAnimation(ActualWidth - EntryColumn.Width.Value, ActualWidth);
                activeStoryboard.Completed += (s, e_i) =>
                {
                    ViewModel.ActiveEntry = null;
                };
            }
            else
            {
                ViewModel.ActiveEntry = null;
            }
        }

        private Button deleteButton;
        private Button editButton;
        private void itemSelected(object sender, SelectionChangedEventArgs e)
        {
            RefreshAppBarButtons();

            ListViewBase listView = (ListViewBase)sender;
            if (listView.SelectedItems.Count == 0)
            {
                BottomAppBar.IsSticky = false;
                BottomAppBar.IsOpen = false;

                if (ViewModel.ActiveEntry != null)
                {
                    btnExitEntry_Click(this, null);
                }
            }
            else
            {
                // This should be handled by itemClick logic already, which calls DBViewModel.Select
                /*if (listView.SelectedItems.Count == 1 && listView.SelectedItem is KdbxEntry)
                {
                    ViewModel.ActiveEntry = (KdbxEntry)listView.SelectedItem;
                }*/
                if (!itemClicked && ViewModel.ActiveEntry != null)
                {
                    // If we previously had an ActiveEntry, and we're either selecting more than 1 item
                    // or a group, kill the active entry.
                    if (listView.SelectedItems.Count == 1 && listView.SelectedItem is KdbxEntry ||
                        listView.SelectedItems.Count > 1)
                    {
                        btnExitEntry_Click(this, null);
                    }
                }

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
            ((ListViewBase)sender).SelectionMode = ListViewSelectionMode.Multiple;
            ((ListViewBase)sender).IsSwipeEnabled = true;
        }

        public void ShowDetails(object sender, EventArgs e)
        {
            Navigator.Navigate(typeof(EntryDetailsView), ViewModel.GetEntryDetailViewModel((KdbxEntry)ViewModel.ActiveEntry));
        }
    }
}
