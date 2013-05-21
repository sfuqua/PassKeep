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
                KdbxString strItem = (KdbxString)item;
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);

                if (!strItem.Protected && strItem.ClearValue.Contains("\n"))
                {
                    element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                }
                else
                {
                    element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
                }
            }
            else
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
