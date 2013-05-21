using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Controls
{
    public sealed partial class ProtectedTextBox : UserControl
    {
        public event EventHandler DataCopied;
        private void onDataCopied()
        {
            if (DataCopied != null)
            {
                DataCopied(this, new EventArgs());
            }
        }

        public static readonly DependencyProperty BoxStyleProperty
            = DependencyProperty.Register("BoxStyle", typeof(Style), typeof(ProtectedTextBox), PropertyMetadata.Create(GetDefaultStyle));
        public Style BoxStyle
        {
            get { return (Style)GetValue(BoxStyleProperty); }
            set { SetValue(BoxStyleProperty, value); }
        }

        public static object GetDefaultStyle()
        {
            return null;
        }

        public static readonly DependencyProperty ShowCopyButtonProperty
            = DependencyProperty.Register("ShowCopyButton", typeof(bool), typeof(ProtectedTextBox), PropertyMetadata.Create(true));
        public bool ShowCopyButton
        {
            get { return (bool)GetValue(ShowCopyButtonProperty); }
            set { SetValue(ShowCopyButtonProperty, value); }
        }

        public static readonly DependencyProperty KStringProperty
            = DependencyProperty.Register("KString", typeof(KdbxString), typeof(ProtectedTextBox), PropertyMetadata.Create(KdbxString.Empty, kstringChanged));
        public KdbxString KString
        {
            get { return (KdbxString)GetValue(KStringProperty); }
            set { SetValue(KStringProperty, value); }
        }

        private void propChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Protected")
            {
                if (PART_ClearBox.FocusState == FocusState.Unfocused)
                {
                    SetClearBoxProtectedState(null, new RoutedEventArgs());
                }
                else
                {
                    SetClearBoxUnprotectedState(null, new RoutedEventArgs());
                }
            }
        }

        private static void kstringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProtectedTextBox box = (ProtectedTextBox)o;

            KdbxString oldStr = (KdbxString)e.OldValue;
            if (oldStr != null)
            {
                oldStr.PropertyChanged -= box.propChangedHandler;
            }

            KdbxString newStr = (KdbxString)e.NewValue;
            if (newStr != null)
            {
                newStr.PropertyChanged += box.propChangedHandler;
                if (newStr.Protected)
                {
                    box.SetClearBoxProtectedState(null, new RoutedEventArgs());
                }
            }
        }

        public Guid Uuid;
        public ProtectedTextBox()
        {
            this.InitializeComponent();
            Uuid = Guid.NewGuid();
            PART_ProtectedBox.DataContext = this;
            PART_ClearBox.DataContext = this;
            PART_CopyButton.DataContext = this;
            SetBinding(KStringProperty, new Binding());
        }

        private void PART_CopyButton_Click(object sender, RoutedEventArgs e)
        {
            KdbxString str = DataContext as KdbxString;
            if (str == null || string.IsNullOrEmpty(str.ClearValue))
            {
                Clipboard.Clear();
                return;
            }

            DataPackage package = new DataPackage();
            package.SetText(str.ClearValue);
            Clipboard.SetContent(package);

            onDataCopied();
        }

        private void SetClearBoxUnprotectedState(object sender, RoutedEventArgs e)
        {
            PART_ProtectedBox.Opacity = 0;
            PART_ClearBox.Opacity = 1;
        }

        private void SetClearBoxProtectedState(object sender, RoutedEventArgs e)
        {
            if (KString.Protected)
            {
                PART_ProtectedBox.Opacity = 1;
                PART_ClearBox.Opacity = 0;
            }
            else
            {
                SetClearBoxUnprotectedState(sender, e);
            }
        }

        private void PART_ClearBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            (DataContext as KdbxString).ClearValue = ((TextBox)sender).Text;
        }
    }
}
