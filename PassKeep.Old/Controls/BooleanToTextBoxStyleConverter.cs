using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Controls
{
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    public sealed class BooleanToTextBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                return null;
            }

            string param = parameter as string;
            if (param == null)
            {
                return null;
            }

            bool bValue = (bool)value;
            bool bSnapped = ApplicationView.Value == ApplicationViewState.Snapped;
            string name = bValue ? "ReadOnly" + param : param;
            name = bSnapped ? name + "Snapped" : name;
            return Application.Current.Resources[name];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
