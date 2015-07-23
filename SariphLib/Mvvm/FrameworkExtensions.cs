using System;
using Windows.UI.Xaml;
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
            var childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var result = (VisualTreeHelper.GetChild(element, i) as FrameworkElement).FindDescendantByName(name);
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
            for (var i = 0; i < childCount; i++)
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
    }
}
