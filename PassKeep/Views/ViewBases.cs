using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using PassKeep.Controls;
using PassKeep.Models.Abstraction;
using PassKeep.ViewModels;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using PassKeep.Common;

namespace PassKeep.Views
{
    public abstract class EntrySearchViewBase : PassKeepPage<EntrySearchViewModel> { }

    public abstract class WelcomeViewBase : PassKeepPage<BasicViewModel> { }
    public abstract class DatabaseUnlockViewBase : PassKeepPage<DatabaseUnlockViewModel> { }
    public abstract class DatabaseViewBase : PassKeepPage<DatabaseViewModel> { }

    public abstract class DetailsViewBase<TViewModel, TModel> : PassKeepPage<TViewModel>
        where TViewModel : DetailsViewModelBase<TModel>
        where TModel : IKeePassNode
    {
        #region StyleConverter
        private sealed class IsReadOnlyToStyleConverter : IValueConverter
        {
            private Style editStyle;
            private Style revertStyle;

            public IsReadOnlyToStyleConverter()
            {
                editStyle = new Style(typeof(Button));
                editStyle.BasedOn = (Style)Application.Current.Resources["EditAppBarButtonStyle"];
                editStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, "Edit Entry"));

                revertStyle = new Style(typeof(Button));
                revertStyle.BasedOn = (Style)Application.Current.Resources["AppBarButtonStyle"];
                revertStyle.Setters.Add(
                    new Setter(
                        AutomationProperties.AutomationIdProperty,
                        "RevertAppBarButton"
                    )
                );
                revertStyle.Setters.Add(
                    new Setter(
                        AutomationProperties.NameProperty,
                        "Revert Entry"
                    )
                );
                revertStyle.Setters.Add(
                    new Setter(
                        Button.ContentProperty,
                        "\uE106"
                    )
                );
            }

            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (!(value is bool))
                {
                    return null;
                }

                // IsReadOnly ? Edit : Revert
                return ((bool)value) ? editStyle : revertStyle;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        public override bool IsProtected
        {
            get { return true; }
        }

        public override Task<bool> Lock()
        {
            throw new NotImplementedException();
            //Navigator.ReplacePage(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, ViewModel.DatabaseViewModel.File));
            //return Task.Run(() => true);
        }

        public DatabaseNavigationViewModel BreadcrumbViewModel
        {
            get;
            protected set;
        }

        public DetailsViewBase()
        {
            toggleButton = new Button();
            toggleButton.SetBinding(
                Button.StyleProperty,
                new Binding
                {
                    Path = new PropertyPath("IsReadOnly"),
                    Converter = new IsReadOnlyToStyleConverter()
                }
            );
            toggleButton.Click += toggleMode;

            saveButton = new Button();
            saveButton.Style = (Style)Application.Current.Resources["SaveAppBarButtonStyle"];
            saveButton.Click += async (s, e) => { await save(); };
        }

        protected Button toggleButton;
        protected Button saveButton;
        protected virtual void toggleMode(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsReadOnly)
            {
                // Clicked "Edit"
                enterEditMode();
            }
            else
            {
                // Clicked "Revert"
                revert();
            }
        }

        #region AppBar buttons

        public override void SetupDefaultAppBarButtons()
        {
            CustomAppBarButtons.Add(toggleButton);
            if (ViewModel != null && !ViewModel.IsReadOnly)
            {
                CustomAppBarButtons.Insert(0, saveButton);
            }
        }

        public override void SetupSnappedAppBarButtons()
        {
            LeftCommands.Visibility = Visibility.Collapsed;
            CustomAppBarButtons.Add(toggleButton);
            if (ViewModel != null && !ViewModel.IsReadOnly)
            {
                CustomAppBarButtons.Insert(0, saveButton);
            }
        }

        #endregion

        protected void enterEditMode()
        {
            ViewModel.IsReadOnly = false;
        }

        protected async Task save()
        {
            bool saved = await ViewModel.Save();
            if (saved)
            {
                ViewModel.IsReadOnly = true;
            }
        }

        protected async void revert()
        {
            if (!(await navigationPrompt()))
            {
                return;
            }

            int i;
            if (ViewModel.GetBackup(out i) == null )
            {
                Frame.Navigate(typeof(DatabaseView), ViewModel.DatabaseViewModel);
                return;
            }

            ViewModel.IsReadOnly = true;
            ViewModel.Revert();
        }

        private void StartedWriteHandler(object sender, CancelableEventArgs e)
        {
            onStartedLoading("Saving...", e.Cancel);
        }

        private void DoneWriteHandler(object sender, EventArgs e)
        {
            onDoneLoading();
        }

        private async Task<bool> navigationPrompt()
        {
            int i;
            if (!ViewModel.IsReadOnly && !ViewModel.Item.Equals(ViewModel.GetBackup(out i)))
            {
                var dialog = new MessageDialog("Do you want to commit any unsaved changes to the database?", "Save changes?")
                {
                    Options = MessageDialogOptions.None,
                };

                IUICommand yesCmd = new UICommand("Yes");
                IUICommand noCmd = new UICommand("No");
                IUICommand cancelCmd = new UICommand("Cancel");

                dialog.Commands.Add(yesCmd);
                dialog.Commands.Add(noCmd);
                dialog.Commands.Add(cancelCmd);

                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 2;

                IUICommand chosenCmd = await dialog.ShowAsync();
                if (chosenCmd == yesCmd)
                {
                    return await ViewModel.Save();
                }
                else if (chosenCmd == noCmd)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected async override void TryGoBack()
        {
            if (await navigationPrompt())
            {
                base.TryGoBack();
            }
        }

        protected async override void TryGoForward()
        {
            if (await navigationPrompt())
            {
                base.TryGoForward();
            }
        }

        private void onPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsReadOnly")
            {
                RefreshAppBarButtons();
            }
        }

        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);

            ViewModel.DatabaseViewModel.StartedWrite += StartedWriteHandler;
            ViewModel.DatabaseViewModel.DoneWrite += DoneWriteHandler;
            ViewModel.PropertyChanged += onPropertyChangedHandler;

            BreadcrumbViewModel = new DatabaseNavigationViewModel(ViewModel.Settings);

            if (!ViewModel.IsReadOnly)
            {
                enterEditMode();
            }
        }

        protected async void breadcrumbClick(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Debug.Assert(clickedGroup != null);
            Debug.Assert(ViewModel != null);
            Debug.Assert(ViewModel.DatabaseViewModel != null);

            if (await navigationPrompt())
            {
                ViewModel.DatabaseViewModel.Select(clickedGroup, true);
                Frame.Navigate(typeof(DatabaseView), ViewModel.DatabaseViewModel);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.DatabaseViewModel.StartedWrite -= StartedWriteHandler;
            ViewModel.DatabaseViewModel.DoneWrite -= DoneWriteHandler;
            ViewModel.PropertyChanged -= onPropertyChangedHandler;
        }

        public override async Task<bool> HandleHotKey(VirtualKey key)
        {
            switch(key)
            {
                case VirtualKey.S:
                    if (ViewModel.IsReadOnly)
                    {
                        return false;
                    }
                    await save();
                    return true;
                case VirtualKey.D:
                    toggleMode(null, null);
                    return true;
                default:
                    return false;
            }
        }
    }

    public abstract class EntryDetailsViewBase : DetailsViewBase<EntryDetailsViewModel, IKeePassEntry> { }
    public abstract class GroupDetailsViewBase : DetailsViewBase<GroupDetailsViewModel, IKeePassGroup> { }
}
