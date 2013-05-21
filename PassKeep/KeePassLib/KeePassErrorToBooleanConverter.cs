using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PassKeep.KeePassLib
{
    public class KeePassErrorToBooleanConverter : IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            KeePassError error = value as KeePassError;
            return (value != null && (error.Error != KdbxParseError.None));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
