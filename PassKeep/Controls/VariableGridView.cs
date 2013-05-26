using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.Common;
using PassKeep.Models;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Controls
{
    /// <summary>
    /// Thanks to Jerry Nixon for code: http://codepaste.net/aopvks
    /// </summary>
    public class VariableGridView : GridView
    {
        public VariableGridView()
            : base()
        {
            Loaded += VariableGridView_Loaded;
        }

        private void VariableGridView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (object item in Items)
            {
                DependencyObject container = ItemContainerGenerator.ContainerFromItem(item);
                SizeFromKdbxString(container, (KdbxString)item);
            }

            this.FindDescendantByType<VariableSizedWrapGrid>().InvalidateMeasure();
        }

        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);
            LayoutUpdated += VariableGridView_LayoutUpdated;
        }

        protected void VariableGridView_LayoutUpdated(object sender, object e)
        {
            LayoutUpdated -= VariableGridView_LayoutUpdated;
            if (Items.Count > 0)
            {
                KdbxString lastItem = Items[Items.Count - 1] as KdbxString;
                SizeFromKdbxString(ItemContainerGenerator.ContainerFromItem(lastItem), lastItem);
                this.FindDescendantByType<VariableSizedWrapGrid>().InvalidateMeasure();
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (item is KdbxString)
            {
                SizeFromKdbxString(ItemContainerGenerator.ContainerFromItem(item), (KdbxString)item);
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
            Border templateBorder = item.FindDescendantByType<Border>();
            if (templateBorder == null)
            {
                item.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 4);

                if (!str.Protected && str.ClearValue.Contains("\n"))
                {
                    item.SetValue(VariableSizedWrapGrid.RowSpanProperty, 4);
                }
                else
                {
                    item.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                }
            }
            else
            {
                templateBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                TextBlock headerBlock = templateBorder.FindDescendantByType<TextBlock>();
                headerBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                ProtectedTextBox contentBox = templateBorder.FindDescendantByType<ProtectedTextBox>();
                contentBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // As a hack I add 5px to the desiredWidth to bump it up if necessary...
                double desiredWidth = Math.Max(headerBlock.DesiredSize.Width, contentBox.DesiredSize.Width) + 5;
                double desiredHeight = headerBlock.DesiredSize.Height + contentBox.DesiredSize.Height;

                VariableSizedWrapGrid vswg = VisualTreeHelper.GetParent(item) as VariableSizedWrapGrid;

                int columns = (int)Math.Ceiling(desiredWidth / vswg.ItemWidth);
                columns = Math.Max(Math.Min(columns, 5), 1);
                item.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, columns);

                int rows = (int)Math.Ceiling(desiredHeight / vswg.ItemHeight);
                rows = Math.Max(Math.Min(rows, 7), 1);
                item.SetValue(VariableSizedWrapGrid.RowSpanProperty, rows);
            }
        }
    }
}
