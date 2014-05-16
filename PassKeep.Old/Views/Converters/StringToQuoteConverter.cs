using System;
using Windows.UI.Xaml.Data;

namespace PassKeep.Views.Converters
{
    public class StringToQuoteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string))
            {
                throw new ArgumentException();
            }

            string text = (string)value;
            return string.Format("\u201c{0}\u201d", text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}