// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// Extension methods for dealing with the XAML VisualTree.
    /// </summary>
    public static class FrameworkExtensions
    {
        /// <summary>
        /// Given a FrameworkElement, find a descendant by x:Name value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement FindDescendantByName(this FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            // Base case - this element is a match
            if (name.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }

            // Iterate over children to recursively search
            int childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                FrameworkElement result = (VisualTreeHelper.GetChild(element, i) as FrameworkElement).FindDescendantByName(name);
                if (result != null)
                {
                    return result;
                }
            }

            // No matches in tree
            return null;
        }

        /// <summary>
        /// Finds the first matching descendant of an object with the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static T FindDescendantByType<T>(this DependencyObject root) where T : UIElement
        {
            if (root == null)
            {
                return null;
            }

            if (root is T)
            {
                return root as T;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                T firstChildOfType = child.FindDescendantByType<T>();
                if (firstChildOfType != null)
                {
                    return firstChildOfType;
                }
            }

            return null;
        }

        /// <summary>
        /// Shows a popup below <paramref name="anchor"/>.
        /// </summary>
        /// <param name="popup">The popup to show.</param>
        /// <param name="anchor">The control to attach the popup to.</param>
        /// <param name="commonAncestor">The nearest common ancestor of the popup and the anchor.</param>
        public static void ShowBelow(this Popup popup, FrameworkElement anchor, UIElement commonAncestor)
        {
            Point coords = anchor.TransformToVisual(commonAncestor).TransformPoint(new Point(0, 0));
            popup.HorizontalOffset = coords.X;
            popup.VerticalOffset = coords.Y + anchor.ActualHeight;
            popup.Width = anchor.Width;
            popup.IsOpen = true;
        }
    }
}
