using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Controls
{
    /// <summary>
    /// Thanks to Jerry Nixon for code: http://codepaste.net/aopvks
    /// </summary>
    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (item is KdbxString)
            {
                SizeFromKdbxString(element, (KdbxString)item);
            }
            else
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            base.PrepareContainerForItemOverride(element, item);
        }

        public static void SizeFromKdbxString(DependencyObject item, KdbxString str)
        {
            item.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);

            if (!str.Protected && str.ClearValue.Contains("\n"))
            {
                item.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
            }
            else
            {
                item.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }
        }
    }
}
