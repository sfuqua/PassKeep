using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Controls
{
    public sealed partial class ToggleableUintField : UserControl
    {
        public DependencyProperty EnabledProperty = 
            DependencyProperty.Register("Enabled", typeof(bool), typeof(ToggleableUintField), PropertyMetadata.Create(true));
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public DependencyProperty DescriptionProperty = 
            DependencyProperty.Register("Description", typeof(object), typeof(ToggleableUintField), PropertyMetadata.Create("Description text"));
        public object Description
        {
            get { return (object)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(uint), typeof(ToggleableUintField), PropertyMetadata.Create(0));
        public uint Value
        {
            get { return (uint)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public DependencyProperty TooltipProperty =
            DependencyProperty.Register("Tooltip", typeof(TextBlock), typeof(ToggleableUintField), PropertyMetadata.Create(new TextBlock()));
        public TextBlock Tooltip
        {
            get { return (TextBlock)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public ToggleableUintField()
        {
            this.InitializeComponent();
            ValueBox.DataContext = this;
            Toggle.DataContext = this;
        }

        private bool ignoreEvent = false;
        private string previousText;
        private int selectionStart = 0, selectionLength = 0;
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreEvent)
            {
                ignoreEvent = false;
                return;
            }

            string text = ValueBox.Text;

            if (text.Length > 0)
            {
                uint result;
                if (!uint.TryParse(text, out result))
                {
                    ignoreEvent = true;
                    ValueBox.Text = previousText;
                    ValueBox.SelectionStart = selectionStart;
                    ValueBox.SelectionLength = selectionLength;
                    return;
                }
            }

            previousText = text;
            selectionStart = ValueBox.SelectionStart;
            selectionLength = ValueBox.SelectionLength;
        }

        private void ValueBox_GotFocus(object sender, RoutedEventArgs e)
        {
            previousText = ValueBox.Text;
            selectionStart = ValueBox.SelectionStart;
            selectionLength = ValueBox.SelectionLength;
        }
    }
}
