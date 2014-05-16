﻿using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

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
            = DependencyProperty.Register("KString", typeof(IProtectedString), typeof(ProtectedTextBox), PropertyMetadata.Create(KdbxString.Empty, kstringChanged));
        public IProtectedString KString
        {
            get { return (IProtectedString)GetValue(KStringProperty); }
            set { SetValue(KStringProperty, value); }
        }

        private void propChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Protected")
            {
                if (KString.Protected)
                {
                    if (PART_ProtectedBox.FocusState == FocusState.Unfocused)
                    {
                        OnLostFocus(null, new RoutedEventArgs());
                    }
                    else
                    {
                        OnGotFocus(null, new RoutedEventArgs());
                    }
                }
                else
                {
                    Deprotect();
                }
            }
            else if (e.PropertyName == "ClearValue")
            {
                if (!KString.Protected)
                {
                    PART_ProtectedBox.TextChanged -= TextChanged;
                    PART_ProtectedBox.Text = KString.ClearValue;
                    PART_ProtectedBox.TextChanged += TextChanged;
                }
            }
        }

        private static void kstringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProtectedTextBox box = (ProtectedTextBox)o;

            IProtectedString oldStr = (IProtectedString)e.OldValue;
            if (oldStr != null)
            {
                oldStr.PropertyChanged -= box.propChangedHandler;
            }

            IProtectedString newStr = (IProtectedString)e.NewValue;
            if (newStr != null)
            {
                newStr.PropertyChanged += box.propChangedHandler;
                if (newStr.Protected)
                {
                    box.Protect();
                }
                else
                {
                    box.Deprotect();
                }
            }
        }

        public ProtectedTextBox()
        {
            this.InitializeComponent();
            PART_ProtectedBox.DataContext = this;
            PART_CopyButton.DataContext = this;
            SetBinding(KStringProperty, new Binding());
        }

        private void PART_CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (KString == null || string.IsNullOrEmpty(KString.ClearValue))
            {
                Clipboard.Clear();
                return;
            }

            DataPackage package = new DataPackage();
            package.SetText(KString.ClearValue);
            Clipboard.SetContent(package);

            onDataCopied();
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (KString.Protected)
            {
                Deprotect();
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (KString.Protected)
            {
                Protect();
            }
        }

        public void Protect()
        {
            PART_ProtectedBox.TextChanged -= TextChanged;
            PART_ProtectedBox.Text = "[Protected]";
            PART_ProtectedBox.FontWeight = FontWeights.Bold;
        }

        public void Deprotect()
        {
            PART_ProtectedBox.TextChanged -= TextChanged;

            if (KString != null)
            {
                PART_ProtectedBox.Text = KString.ClearValue ?? string.Empty;
            }
            else
            {
                PART_ProtectedBox.Text = string.Empty;
            }
            PART_ProtectedBox.ClearValue(TextBox.FontWeightProperty);
            PART_ProtectedBox.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            (DataContext as IProtectedString).ClearValue = ((TextBox)sender).Text;
        }
    }
}