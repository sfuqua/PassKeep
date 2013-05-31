using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PassKeep.Common
{
    public sealed class ForegroundColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is Color))
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is SolidColorBrush))
            {
                return null;
            }

            return ((SolidColorBrush)value).Color;
        }
    }

    public sealed class BackgroundColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is Color))
            {
                return App.Current.Resources["ListViewItemPlaceholderBackgroundThemeBrush"];
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is SolidColorBrush))
            {
                return null;
            }

            return ((SolidColorBrush)value).Color;
        }
    }
}
