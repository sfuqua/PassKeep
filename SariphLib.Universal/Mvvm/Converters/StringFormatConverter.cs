using System;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm.Converters
{
    /// <summary>
    /// A converter for formatting strings in bindings.
    /// </summary>
    public class StringFormatConverter : IValueConverter
    {
        /// <summary>
        /// Formats <paramref name="value"/> with format string <paramref name="parameter"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return String.Empty;
            }

            if (parameter == null)
            {
                return value.ToString();
            }

            string strFormat = parameter.ToString();
            return String.Format(strFormat, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
