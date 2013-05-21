using PassKeep.Common;
using PassKeep.Models;
using PassKeep.ViewModels;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EntryDetailsView : EntryDetailsViewBase
    {
        private const double PopupHeight = 200;
        private const double PopupWidth = 200;
        public PasswordGenViewModel PasswordGenViewModel { get; set; }

        public EntryDetailsView()
        {
            this.InitializeComponent();
            
            copyFieldButton = new Button();
            Style btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["CopyAppBarButtonStyle"];
            btnStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, "Copy Field Value"));
            copyFieldButton.Style = btnStyle;
            copyFieldButton.Click += (s, e_i) =>
            {
                if (fieldsList.SelectedItems.Count != 1)
                {
                    return;
                }

                DataPackage package = new DataPackage();
                package.SetText(((KdbxString)fieldsList.SelectedItem).ClearValue);
                Clipboard.SetContent(package);
            };

            deleteFieldButton = new Button();
            btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["DeleteAppBarButtonStyle"];
            btnStyle.Setters.Add(new Setter(Button.ContentProperty, "\uE107"));
            btnStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, "Delete Field"));

            deleteFieldButton.Style = btnStyle;
            deleteFieldButton.Click += (s, e_i) =>
            {
                deleteSelection();
            };

            editFieldButton = new Button();
            editFieldButton.Click += (s, e_i) =>
            {
                KdbxString clicked = (KdbxString)fieldsList.SelectedItem;

                var container = (UIElement)fieldsList.ItemContainerGenerator.ContainerFromItem(fieldsList.SelectedItem);
                var transform = container.TransformToVisual(null);
                var transformedPoint = transform.TransformPoint(new Point());

                kdbxStringPopupTL(transformedPoint.Y, transformedPoint.X, clicked);

                /*var bounds = Window.Current.CoreWindow.Bounds;
                kdbxStringPopupBR(
                    (bounds.Bottom - bounds.Top) - BottomAppBar.ActualHeight,
                    (bounds.Right - bounds.Left),
                    clicked
                );*/
            };
            btnStyle = new Style(typeof(Button));
            btnStyle.BasedOn = (Style)Application.Current.Resources["EditAppBarButtonStyle"];
            btnStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, "Edit Field"));
            editFieldButton.Style = btnStyle;
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            base.LoadState(navigationParameter, pageState);
            PasswordGenViewModel = new PasswordGenViewModel(ViewModel.Settings);
            BreadcrumbViewModel.SetEntry(ViewModel.Item);
        }

        public override void SetupDefaultAppBarButtons()
        {
            base.SetupDefaultAppBarButtons();
            if (fieldsList.SelectedItem != null)
            {
                CustomAppBarButtons.Insert(0, copyFieldButton);
                if (!ViewModel.IsReadOnly)
                {
                    CustomAppBarButtons.Insert(0, editFieldButton);
                    CustomAppBarButtons.Insert(0, deleteFieldButton);
                }
            }
        }

        public override void SetupPortraitAppBarButtons()
        {
            LeftCommands.Visibility = Visibility.Collapsed;
            SetupDefaultAppBarButtons();
        }

        protected override void toggleMode(object sender, RoutedEventArgs e)
        {
            base.toggleMode(sender, e);
            passwordGeneratorPopup.IsOpen = false;
        }

        private void deleteSelection()
        {
            if (fieldsList.SelectedItems.Count == 0)
            {
                return;
            }

            // TODO: Allow multiple
            ViewModel.Item.Fields.RemoveAt(fieldsList.SelectedIndex);
        }

        Button copyFieldButton;
        Button deleteFieldButton;
        Button editFieldButton;
        private void fieldsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAppBarButtons();

            if (e.AddedItems.Count == 1)
            {
                BottomAppBar.IsSticky = true;
                BottomAppBar.IsOpen = true;
            }
            else
            {
                BottomAppBar.IsSticky = false;
                BottomAppBar.IsOpen = false;
            }
        }

        private void NewField_Click(object sender, RoutedEventArgs e)
        {
            var transform = newField_btn.TransformToVisual(null);
            var transformedPoint = transform.TransformPoint(new Point());

            kdbxStringPopupTL(
                transformedPoint.Y,
                transformedPoint.X
            );
        }

        private void OpenPwGen_Click(object sender, RoutedEventArgs e)
        {
            passwordGeneratorPopup.IsOpen = true;
        }

        private async void GeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsReadOnly)
            {
                string newPassword = await PasswordGenViewModel.Generate();
                ViewModel.Item.Password.ClearValue = newPassword;
            }

            passwordGeneratorPopup.IsOpen = false;
        }

        private async void GenerateToClipboard_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = await PasswordGenViewModel.Generate();

            DataPackage pkg = new DataPackage();
            pkg.SetText(newPassword);
            try
            {
                Clipboard.SetContent(pkg);
            }
            catch (Exception)
            { } // If the app somehow lost focus, SetContent will fail. It's fine for this to silently give up.

            passwordGeneratorPopup.IsOpen = false;
        }

        private Popup getKdbxStringPopup(KdbxString str = null)
        {
            KdbxString dummy = (str != null ? str.Clone() : new KdbxString(string.Empty, string.Empty, ViewModel.DatabaseViewModel.GetRng()));
            Popup p = new Popup
            {
                IsLightDismissEnabled = true
            };

            Thickness textboxMargin = new Thickness(4, 0, 4, 4);
            TextBox nameBox = new TextBox { Name = "PART_NameBox", Margin = textboxMargin };
            nameBox.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("Key"), Mode = BindingMode.TwoWay });

            string error = string.Empty;
            List<string> invalidNames = new List<string> { "UserName", "Password", "Title", "Notes", "URL" };
            Func<bool> canSave = () =>
            {
                error = string.Empty;
                if (string.IsNullOrEmpty(dummy.Key))
                {
                    error = "Please enter a field name.";
                    return false;
                }

                if (invalidNames.Contains(dummy.Key))
                {
                    error = "That field name is reserved, please use a different one.";
                    return false;
                }

                if ((str == null || str.Key != dummy.Key)
                    && ViewModel.Item.Fields.Select(f => f.Key).Contains(dummy.Key))
                {
                    error = "That field name is already being used, please use a different one.";
                    return false;
                }

                return true;
            };

            Action saveAction = () =>
            {
                if (str == null)
                {
                    ViewModel.Item.Fields.Add(dummy);
                }
                else
                {
                    if (str.Key != dummy.Key)
                    {
                        str.Key = dummy.Key;
                    }
                    if (str.Protected != dummy.Protected)
                    {
                        str.Protected = dummy.Protected;
                    }
                    if (str.ClearValue != dummy.ClearValue)
                    {
                        str.ClearValue = dummy.ClearValue;
                    }
                }

                p.IsOpen = false;
            };

            DelegateCommand saveCommand = new DelegateCommand(canSave, saveAction);
            nameBox.TextChanged += (s, e) =>
            {
                dummy.Key = nameBox.Text;
                saveCommand.RaiseCanExecuteChanged();
            };

            KeyEventHandler textboxKeyEventHandler = (sender, e) =>
            {
                if (e.Key == VirtualKey.Enter)
                {
                    if (saveCommand.CanExecute(null))
                    {
                        saveCommand.Execute(null);
                    }
                }
            };

            nameBox.KeyUp += textboxKeyEventHandler;

            Border border = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
                Padding = new Thickness(4),
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0))
            };

            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = BottomAppBar.Background,
                Height = PopupHeight,
                Width = PopupWidth,
                DataContext = dummy
            };
            border.Child = panel;

            Style labelStyle = (Style)Application.Current.Resources["EntryFieldLabel"];
            
            panel.Children.Add(
                new TextBlock
                {
                    Text = "Name",
                    Style = labelStyle
                }
            );
            panel.Children.Add(nameBox);

            panel.Children.Add(
                new TextBlock
                {
                    Text = "Value",
                    Style = labelStyle
                }
            );

            TextBox valueBox = new TextBox { Margin = textboxMargin };
            valueBox.KeyUp += textboxKeyEventHandler;
            valueBox.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("ClearValue"), Mode = BindingMode.TwoWay });
            panel.Children.Add(valueBox);

            CheckBox protectedBox = new CheckBox { Content = "Protect in memory" };
            protectedBox.SetBinding(CheckBox.IsCheckedProperty, new Binding { Path = new PropertyPath("Protected"), Mode = BindingMode.TwoWay });
            panel.Children.Add(protectedBox);
            Button saveFieldBtn = new Button
            {
                Content = "Save",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            dynamic btnProxy = saveFieldBtn;
            btnProxy.Command = saveCommand;
            panel.Children.Add(saveFieldBtn);

            p.Child = border;
            p.Opened += (s, e) =>
            {
                Popup thisPopup = (Popup)s;
                ((TextBox)((FrameworkElement)thisPopup.Child).FindName("PART_NameBox")).Focus(FocusState.Programmatic);
            };

            return p;
        }

        private void kdbxStringPopupTL(double top, double left, KdbxString str = null)
        {
            Popup p = getKdbxStringPopup(str);

            var bounds = Window.Current.CoreWindow.Bounds;
            double maxBottom = (bounds.Bottom - bounds.Top);
            if (BottomAppBar.IsOpen)
            {
                maxBottom -= BottomAppBar.ActualHeight;
            }

            if (top + PopupHeight >= maxBottom)
            {
                top = maxBottom - PopupHeight;
            }

            p.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            p.HorizontalOffset = left;
            p.VerticalOffset = top;

            p.IsOpen = true;
        }

        private void kdbxStringPopupBR(double bottom, double right, KdbxString str = null)
        {
            Popup p = getKdbxStringPopup(str);

            var bounds = Window.Current.CoreWindow.Bounds;
            double maxBottom = (bounds.Bottom - bounds.Top);
            if (BottomAppBar.IsOpen)
            {
                maxBottom -= BottomAppBar.ActualHeight;
            }

            if (bottom >= maxBottom)
            {
                bottom = maxBottom;
            }

            p.IsOpen = true;
            p.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            p.HorizontalOffset = right - p.DesiredSize.Width;
            p.VerticalOffset = bottom - p.DesiredSize.Height;
        }

        private void OverrideUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
           ViewModel.Item.OverrideUrl = ((TextBox)sender).Text;
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Item.Notes.ClearValue = ((TextBox)sender).Text;
        }

        private void Tags_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Item.Tags = ((TextBox)sender).Text;
        }

        public override void HandleDelete()
        {
            deleteSelection();
        }

        public override void HandleHotKey(VirtualKey key)
        {
            switch(key)
            {
                case VirtualKey.C:
                    break;
                default:
                    base.HandleHotKey(key);
                    break;
            }
        }
    }
}
