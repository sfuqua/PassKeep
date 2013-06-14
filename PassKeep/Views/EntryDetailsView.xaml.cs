using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PassKeep.Controls;
using PassKeep.Models;
using PassKeep.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EntryDetailsView : EntryDetailsViewBase
    {
        public PasswordGenViewModel PasswordGenViewModel { get; set; }
        public FieldEditViewModel FieldEditViewModel { get; set; }

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
                if (fieldsGridView.SelectedItems.Count != 1)
                {
                    return;
                }

                DataPackage package = new DataPackage();
                package.SetText(((KdbxString)fieldsGridView.SelectedItem).ClearValue);
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
                if (ViewModel.IsReadOnly)
                {
                    ViewModel.IsReadOnly = false;
                }

                KdbxString clicked = (KdbxString)fieldsGridView.SelectedItem;

                var container = (UIElement)fieldsGridView.ItemContainerGenerator.ContainerFromItem(fieldsGridView.SelectedItem);
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

            FieldEditViewModel = new FieldEditViewModel(ViewModel.Item, ViewModel.DatabaseViewModel.GetRng(), ViewModel.Settings);
            FieldEditViewModel.FieldChanged += fieldChanged;
            FieldEditViewModel.ValidationError += fieldEditValidationError;

            BreadcrumbViewModel.SetEntry(ViewModel.Item);

            InputPane.GetForCurrentView().Showing += paneShowing;
            InputPane.GetForCurrentView().Hiding += paneHiding;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (FieldEditViewModel != null)
            {
                FieldEditViewModel.FieldChanged -= fieldChanged;
                FieldEditViewModel.ValidationError -= fieldEditValidationError;
            }

            InputPane.GetForCurrentView().Showing -= paneShowing;
            InputPane.GetForCurrentView().Hiding -= paneHiding;

            base.OnNavigatedFrom(e);
        }

        private double fieldEditPopupOcclusion = 0;
        private double pwGenPopupOcclusion = 0;
        private void paneShowing(object sender, InputPaneVisibilityEventArgs e)
        {
            Debug.WriteLine("InputPane is now showing.");
            calculatePopupOcclusion(fieldEditPopup, ref fieldEditPopupOcclusion, e.OccludedRect);
            calculatePopupOcclusion(passwordGeneratorPopup, ref pwGenPopupOcclusion, e.OccludedRect);
        }

        private void calculatePopupOcclusion(Popup p, ref double storage, Rect occlusion)
        {
            if (!p.IsOpen)
            {
                storage = 0;
                return;
            }

            occlusion.Intersect(new Rect(p.HorizontalOffset, p.VerticalOffset, p.Width, p.Height));
            if (occlusion.Height > 0)
            {
                double originalOffset = p.VerticalOffset;
                p.VerticalOffset = Math.Max(0, p.VerticalOffset - occlusion.Height);
                storage = originalOffset - p.VerticalOffset;
                Debug.WriteLine("Popup {0} found to be occluded by {1}px.", p.Name, storage);
            }
        }

        private void paneHiding(object sender, InputPaneVisibilityEventArgs e)
        {
            Debug.WriteLine("InputPane is now hiding.");

            if (fieldEditPopup.IsOpen && fieldEditPopupOcclusion > 0)
            {
                Debug.WriteLine("Moving occluded field edit popup back down {0}px.", fieldEditPopupOcclusion);
                fieldEditPopup.VerticalOffset += fieldEditPopupOcclusion;
            }

            if (passwordGeneratorPopup.IsOpen && pwGenPopupOcclusion > 0)
            {
                Debug.WriteLine("Moving occluded password generator popup back down {0}px.", pwGenPopupOcclusion);
                passwordGeneratorPopup.VerticalOffset += pwGenPopupOcclusion;
            }

            fieldEditPopupOcclusion = 0;
            pwGenPopupOcclusion = 0;
        }

        private void fieldEditValidationError(object sender, ValidationErrorEventArgs e)
        {
            // Not currently doing anything with this error.
        }

        private void fieldChanged(object sender, FieldChangedEventArgs e)
        {
            GridViewItem container = fieldsGridView.ItemContainerGenerator.ContainerFromItem(e.Field) as GridViewItem;
            VariableGridView.SizeFromProtectedString(container, e.Field);
            VariableSizedWrapGrid vswg = VisualTreeHelper.GetParent(container) as VariableSizedWrapGrid;
            vswg.InvalidateMeasure();
        }

        public override void SetupDefaultAppBarButtons()
        {
            base.SetupDefaultAppBarButtons();
            if (fieldsGridView.SelectedItem != null)
            {
                CustomAppBarButtons.Insert(0, copyFieldButton);
                CustomAppBarButtons.Insert(0, editFieldButton);
                if (!ViewModel.IsReadOnly)
                {
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
            if (fieldsGridView.SelectedItems.Count == 0)
            {
                return;
            }

            // TODO: Allow multiple
            ViewModel.Item.Fields.RemoveAt(fieldsGridView.SelectedIndex);
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

        private void kdbxStringPopupTL(double top, double left, KdbxString str = null)
        {
            var bounds = Window.Current.CoreWindow.Bounds;
            double maxBottom = (bounds.Bottom - bounds.Top);
            if (BottomAppBar.IsOpen)
            {
                maxBottom -= BottomAppBar.ActualHeight;
            }

            if (top + fieldEditPopup.Height >= maxBottom)
            {
                top = maxBottom - fieldEditPopup.Height;
            }

            fieldEditPopup.HorizontalOffset = left;
            fieldEditPopup.VerticalOffset = top;
            FieldEditViewModel.Edit(str);
        }

        private void kdbxStringPopupBR(double bottom, double right, KdbxString str = null)
        {
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

            fieldEditPopup.HorizontalOffset = right - fieldEditPopup.Width;
            fieldEditPopup.VerticalOffset = bottom - fieldEditPopup.Height;
            FieldEditViewModel.Edit(str);
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

        public override Task<bool> HandleDelete()
        {
            if (!(FocusManager.GetFocusedElement() is TextBox))
            {
                deleteSelection();
                return Task.Run(() => true);
            }
            else
            {
                return Task.Run(() => false);
            }
        }

        public override Task<bool> HandleHotKey(VirtualKey key)
        {
            switch(key)
            {
                case VirtualKey.C:
                    return Task.Run(() => false);
                default:
                    return base.HandleHotKey(key);
            }
        }

        private void fieldEdit_NameTextChanged(object sender, RoutedEventArgs e)
        {
            FieldEditViewModel.WorkingCopy.Key = fieldEdit_NameBox.Text;
            FieldEditViewModel.SaveCommand.RaiseCanExecuteChanged();
        }

        private void fieldEdit_ValueTextChanged(object sender, RoutedEventArgs e)
        {
            FieldEditViewModel.WorkingCopy.ClearValue = fieldEdit_ValueBox.Text;
            FieldEditViewModel.SaveCommand.RaiseCanExecuteChanged();
        }

        private void fieldEdit_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var shiftState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                if (sender == fieldEdit_NameBox && (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    e.Handled = false;
                }
                else if (FieldEditViewModel.SaveCommand.CanExecute(null))
                {
                    e.Handled = true;
                    FieldEditViewModel.SaveCommand.Execute(null);
                }
            }
        }

        private void fieldEditPopup_Opened(object sender, object e)
        {
            fieldEdit_NameBox.Focus(FocusState.Programmatic);
        }
    }
}
