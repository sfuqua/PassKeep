using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Infrastructure;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// A TextBox that optionally shows "Protected" placeholder text when the string it contains is secure.
    /// </summary>
    public sealed partial class ProtectedTextBox : UserControl
    {
        /// <summary>
        /// DependencyProperty that styles the overall TextBox.
        /// </summary>
        public static readonly DependencyProperty BoxStyleProperty
            = DependencyProperty.Register("BoxStyle", typeof(Style), typeof(ProtectedTextBox), PropertyMetadata.Create((Style)null));

        /// <summary>
        /// DependencyProperty for the string wrapped by this control.
        /// </summary>
        public static readonly DependencyProperty ProtectedStringProperty = DependencyProperty.Register(
            "ProtectedString",
            typeof(IProtectedString),
            typeof(ProtectedTextBox),
            PropertyMetadata.Create(KdbxString.Empty, ProtectedStringChanged)
        );

        /// <summary>
        /// DependencyProperty for whether the TextBox is read-only or not.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ProtectedTextBox), PropertyMetadata.Create(false, IsReadOnlyChanged));

        /// <summary>
        /// Handles changes to <see cref="ProtectedString"/>.
        /// </summary>
        /// <param name="o">This control.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private static void ProtectedStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProtectedTextBox thisControl = (ProtectedTextBox)o;

            IProtectedString oldStr = (IProtectedString)e.OldValue;
            if (oldStr != null)
            {
                oldStr.PropertyChanged -= thisControl.WrappedStringPropertyChangeHandler;
            }

            IProtectedString newStr = (IProtectedString)e.NewValue;
            if (newStr != null)
            {
                newStr.PropertyChanged += thisControl.WrappedStringPropertyChangeHandler;
                if (newStr.Protected)
                {
                    thisControl.Protect();
                }
                else
                {
                    thisControl.Deprotect();
                }
            }
        }

        /// <summary>
        /// Handles changes to <see cref="IsReadOnly"/>.
        /// </summary>
        /// <param name="o">This control.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private static void IsReadOnlyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProtectedTextBox thisControl = (ProtectedTextBox)o;
            thisControl.PART_ProtectedBox.IsReadOnly = (bool)e.NewValue;
        }

        private string protectedPlaceholder;

        public ProtectedTextBox()
        {
            InitializeComponent();
            PART_ProtectedBox.DataContext = this;
            SetBinding(ProtectedStringProperty, new Binding());

            this.protectedPlaceholder = ResourceLoader.GetForCurrentView().GetString("ProtectedStringPlaceholder");
        }

        /// <summary>
        /// Style for the protected TextBox.
        /// </summary>
        public Style BoxStyle
        {
            get { return (Style)GetValue(BoxStyleProperty); }
            set { SetValue(BoxStyleProperty, value); }
        }

        /// <summary>
        /// The string wrapped by this control.
        /// </summary>
        public IProtectedString ProtectedString
        {
            get { return (IProtectedString)GetValue(ProtectedStringProperty); }
            set { SetValue(ProtectedStringProperty, value); }
        }

        /// <summary>
        /// Whether the TextBox is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Handles changes to the "Protected" property of the wrapped string.
        /// </summary>
        /// <param name="sender">The wrapped protected string.</param>
        /// <param name="e">EventArgs for the change.</param>
        private void WrappedStringPropertyChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            Dbg.Assert(sender == ProtectedString);
            IProtectedString wrappedStr = (IProtectedString)sender;

            if (e.PropertyName == "Protected")
            {
                // If the string is newly protected, we need to adjust the current state by deferring to our focus handlers.
                // If it is newly unprotected, clear the protection of this control.
                if (wrappedStr.Protected)
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
                // If the value of the string changed and we're not protected, update the string.
                if (!wrappedStr.Protected)
                {
                    PART_ProtectedBox.TextChanged -= TextChanged;
                    PART_ProtectedBox.Text = wrappedStr.ClearValue;
                    PART_ProtectedBox.TextChanged += TextChanged;
                }
            }
        }

        /// <summary>
        /// Handles revealing the string on focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (ProtectedString.Protected)
            {
                Deprotect();
            }
        }

        /// <summary>
        /// Handles hiding the string on lost focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (ProtectedString.Protected)
            {
                Protect();
            }
        }

        /// <summary>
        /// Hides the string's text and unsubscribes from any text changes.
        /// </summary>
        public void Protect()
        {
            PART_ProtectedBox.TextChanged -= TextChanged;
            PART_ProtectedBox.Text = this.protectedPlaceholder;
            PART_ProtectedBox.FontWeight = FontWeights.Bold;
        }

        /// <summary>
        /// Reveals the string's text.
        /// </summary>
        public void Deprotect()
        {
            PART_ProtectedBox.TextChanged -= TextChanged;

            if (ProtectedString != null)
            {
                PART_ProtectedBox.Text = ProtectedString.ClearValue ?? string.Empty;
            }
            else
            {
                PART_ProtectedBox.Text = string.Empty;
            }

            PART_ProtectedBox.ClearValue(TextBox.FontWeightProperty);
            PART_ProtectedBox.TextChanged += TextChanged;
        }

        /// <summary>
        /// Updates the bound string in real-time instead of waiting for focus changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProtectedString == null)
            {
                return;
            }

            TextBox textBox = (TextBox)sender;
            Dbg.Assert(textBox == this.PART_ProtectedBox);

            ProtectedString.ClearValue = textBox.Text;
        }
    }
}
