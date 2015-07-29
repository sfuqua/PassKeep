using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    public sealed partial class CollapsibleTextBlurb : UserControl
    {
        /// <summary>
        /// DependencyProperty for the HeaderText.
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                "HeaderText",
                typeof(string),
                typeof(CollapsibleTextBlurb),
                PropertyMetadata.Create("HeaderText")
            );

        /// <summary>
        /// DependencyProperty for the Header's style.
        /// </summary>
        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register(
                "HeaderStyle",
                typeof(Style),
                typeof(CollapsibleTextBlurb),
                PropertyMetadata.Create((object)null)
            );

        public CollapsibleTextBlurb()
        {
            this.InitializeComponent();
        }

        public string HeaderText
        {
            get { return GetValue(HeaderTextProperty) as string; }
            set { SetValue(HeaderTextProperty, value); }
        }

        public Style HeaderStyle
        { 
            get { return GetValue(HeaderStyleProperty) as Style; }
            set { SetValue(HeaderStyleProperty, value); }
        }
    }

    /// <summary>
    /// Converter helper for mapping the checked state of the button to its glyph.
    /// </summary>
    public sealed class CheckedToGlyphConverter : IValueConverter
    {
        public const string IndeterminateGlyph = "?";
        public const string CollapsedGlyph = "+";
        public const string OpenedGlyph = "-";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? bValue = value as bool?;
            if (bValue == null)
            {
                return IndeterminateGlyph;
            }

            return bValue.Value ? OpenedGlyph : CollapsedGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
