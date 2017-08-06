// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SariphLib.Mvvm;
using Windows.UI.Xaml.Media;

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// A GridView extension that uses VariableSizedWrapGrid in order to provide different sized containers
    /// for protected "Fields" for a KeePass entry.
    /// The GridView's items are assumed to have a specific structure - a Border containing a TextBlock header
    /// and a ProtectedTextBox content box.
    /// </summary>
    /// <remarks>
    /// Thanks to Jerry Nixon for starter code: http://codepaste.net/aopvks
    /// </remarks>
    public class EntryFieldGridView : GridView
    {
        /// <summary>
        /// Hooks up the "Loaded" event handler for the GridView in order to measure
        /// the size of each item.
        /// </summary>
        public EntryFieldGridView()
            : base()
        {
            Loaded += VariableGridView_Loaded;
        }

        /// <summary>
        /// Adjusts the size of <paramref name="item"/> based on how large it will be to fit
        /// the given <paramref name="str"/> within itself.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="str"></param>
        public static void SizeFromProtectedString(DependencyObject item, IProtectedString str)
        {
            // Find the parent Border within the item
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

        /// <summary>
        /// When items are added or removed, catch a ride on the next layout update.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);
            LayoutUpdated += VariableGridView_LayoutUpdated;
        }

        /// <summary>
        /// Handles sizing each individual container for items.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="item"></param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (item is IProtectedString)
            {
                SizeFromProtectedString(ContainerFromItem(item), (IProtectedString)item);
            }
            else
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Gets an initial size for the contents of the GridView at load time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VariableGridView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (object item in Items)
            {
                DependencyObject container = ContainerFromItem(item);
                SizeFromProtectedString(container, (IProtectedString)item);
            }

            this.FindDescendantByType<VariableSizedWrapGrid>().InvalidateMeasure();
        }

        /// <summary>
        /// Handles interfering with layout to resize containers as needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VariableGridView_LayoutUpdated(object sender, object e)
        {
            LayoutUpdated -= VariableGridView_LayoutUpdated;
            if (Items.Count > 0)
            {
                // Assuming an item was added (because removed items were already appropriately sized),
                // adjust the size of the last field.
                IProtectedString lastItem = Items[Items.Count - 1] as IProtectedString;
                DependencyObject container = ContainerFromItem(lastItem);
                if (container != null)
                {
                    SizeFromProtectedString(container, lastItem);
                }
                this.FindDescendantByType<VariableSizedWrapGrid>().InvalidateMeasure();
            }
        }
    }
}
