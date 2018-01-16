// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Kdf;
using PassKeep.Lib.KeePass.SecurityTokens;
using PassKeep.Lib.ViewModels;
using SariphLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    public sealed partial class DatabaseSettingsControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(IDatabaseSettingsViewModel),
            typeof(DatabaseSettingsControl),
            PropertyMetadata.Create((DatabaseSettingsViewModel)null, ViewModelChanged)
        );

        /// <summary>
        /// DependencyProperty that styles the labels.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty
            = DependencyProperty.Register("LabelStyle", typeof(Style), typeof(DatabaseSettingsControl), PropertyMetadata.Create((Style)null));

        public DatabaseSettingsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Settings ViewModel used by this control.
        /// </summary>
        public IDatabaseSettingsViewModel ViewModel
        {
            get { return (IDatabaseSettingsViewModel)GetValue(ViewModelProperty); }
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

        public IReadOnlyList<EncryptionAlgorithm> EncryptionAlgs =>
            new List<EncryptionAlgorithm>
            {
                EncryptionAlgorithm.Aes,
                EncryptionAlgorithm.ChaCha20
            };

        public IReadOnlyList<Guid> KdfAlgs =>
            new List<Guid>
            {
                AesParameters.AesUuid,
                Argon2Parameters.Argon2Uuid
            };

        /// <summary>
        /// Handles changes to <see cref="ViewModel"/>.
        /// </summary>
        /// <param name="o">This control.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private static void ViewModelChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DatabaseSettingsControl thisControl = (DatabaseSettingsControl)o;

            IDatabaseSettingsViewModel oldVm = (IDatabaseSettingsViewModel)e.OldValue;
            if (oldVm != null)
            {
                oldVm.PropertyChanged -= thisControl.ViewModelPropertyChangedHandler;
            }

            IDatabaseSettingsViewModel newVm = (IDatabaseSettingsViewModel)e.NewValue;
            if (newVm != null)
            {
                newVm.PropertyChanged += thisControl.ViewModelPropertyChangedHandler;
                thisControl.UpdateKdfSettings();
            }
        }

        private void ViewModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            DebugHelper.Assert(sender == ViewModel);
            IDatabaseSettingsViewModel viewModel = (IDatabaseSettingsViewModel)sender;
            if (e.PropertyName == nameof(ViewModel.KdfGuid))
            {
                UpdateKdfSettings();
            }
        }

        private void UpdateKdfSettings()
        {
            if (ViewModel.KdfGuid == Argon2Parameters.Argon2Uuid)
            {
                this.ArgonParameters.Visibility = Visibility.Visible;
            }
            else
            {
                this.ArgonParameters.Visibility = Visibility.Collapsed;
            }
        }

        private async void EncryptionRoundsOneSecond_Click(object sender, RoutedEventArgs e)
        {
            this.controlRoot.Focus(FocusState.Programmatic);
            this.EncryptionRoundsOneSecond.IsEnabled = false;
            this.KdfIterations.IsEnabled = false;

            ulong rounds = await ViewModel.GetKdfParameters().CreateEngine().ComputeOneSecondDelay();
            int value = (int)Math.Min(rounds, (ulong)Int32.MaxValue);
            this.KdfIterations.Value = value;

            this.KdfIterations.IsEnabled = true;
            this.EncryptionRoundsOneSecond.IsEnabled = true;
        }
    }
}
