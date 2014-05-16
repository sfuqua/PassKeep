using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Views.Converters
{
    public class ButtonedOrReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                throw new ArgumentException();
            }

            bool bValue = (bool)value;
            string name = bValue ? "ButtonedFieldBox" : "ReadOnlyFieldBox";
            return Application.Current.Resources[name];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}