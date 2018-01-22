// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    public sealed partial class MasterKeyFieldsControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(IMasterKeyViewModel),
            typeof(MasterKeyFieldsControl),
            PropertyMetadata.Create((IMasterKeyViewModel)null, ViewModelChanged)
        );

        /// <summary>
        /// DependencyProperty that styles the labels.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty
            = DependencyProperty.Register("LabelStyle", typeof(Style), typeof(MasterKeyFieldsControl), PropertyMetadata.Create((Style)null));

        private bool capsLockLocked;

        public MasterKeyFieldsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Settings ViewModel used by this control.
        /// </summary>
        public IMasterKeyViewModel ViewModel
        {
            get { return (IMasterKeyViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Style for the labels.
        /// </summary>
        public Style LabelStyle
        {
            get { return (Style)GetValue(LabelStyleProperty); }
            set { SetValue(LabelStyleProperty, value); }
        }

        /// <summary>
        /// Allows a page to inform the <see cref="MasterKeyFieldsControl"/> about changes in caps lock state.
        /// This is used to control the visibility of the caps lock warning popup.
        /// </summary>
        /// <param name="isLocked">The current state of caps lock.</param>
        public void NotifyCapsLockState(bool isLocked)
        {
            DebugHelper.Trace($"Recorded change in caps lock state. New state: {isLocked}");

            this.capsLockLocked = isLocked;

            if (!isLocked)
            {
                this.CapsLockPopup.IsOpen = false;
            }
            else
            {
                if (this.PasswordInput.FocusState != FocusState.Unfocused)
                {
                    this.CapsLockPopup.ShowBelow(this.PasswordInput, this.ControlRoot);
                }
                else if (this.PasswordConfirm.FocusState != FocusState.Unfocused)
                {
                    this.CapsLockPopup.ShowBelow(this.PasswordConfirm, this.ControlRoot);
                }
            }
        }

        /// <summary>
        /// Handles changes to <see cref="ViewModel"/>.
        /// </summary>
        /// <param name="o">This control.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private static void ViewModelChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MasterKeyFieldsControl thisControl = (MasterKeyFieldsControl)o;
            IMasterKeyViewModel oldVm = (IMasterKeyViewModel)e.OldValue;
            IMasterKeyViewModel newVm = (IMasterKeyViewModel)e.NewValue;
            // Clean up any events on the old one and register as needed for the new one.
        }

        /// <summary>
        /// Handles showing the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PasswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void PasswordInput_GotFocus(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender is PasswordBox);
            if (this.capsLockLocked)
            {
                this.CapsLockPopup.ShowBelow((PasswordBox)sender, this.ControlRoot);
            }
        }

        /// <summary>
        /// Handles hiding the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PaswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender is PasswordBox);
            this.CapsLockPopup.IsOpen = false;
        }

        /// <summary>
        /// Updates the bound master password value for every keystroke.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender == this.PasswordInput);
            ViewModel.MasterPassword = ((PasswordBox)sender).Password;
        }

        /// <summary>
        /// Handles the ENTER key for the password input boxes.
        /// </summary>
        /// <param name="sender">The password box.</param>
        /// <param name="e">EventArgs for the KeyUp event.</param>
        private void PasswordInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (ViewModel.ConfirmCommand.CanExecute(null))
                {
                    DebugHelper.Trace($"{GetType()} got [ENTER], attempting to set database credentials...");
                    ViewModel.ConfirmCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Updates the bound confirmed password value for every keystroke.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender == this.PasswordConfirm);
            ViewModel.ConfirmedPassword = ((PasswordBox)sender).Password;
        }

        /// <summary>
        /// Enables nulling out of the keyfile by deleting all text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyFileBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ViewModel.KeyFile = null;
            }
        }
    }
}
