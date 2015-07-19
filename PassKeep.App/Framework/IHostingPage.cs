using Microsoft.Practices.Unity;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Framework
{
    /// <summary>
    /// A page that hosts other content and provides access to the <see cref="Frame"/>
    /// it uses.
    /// </summary>
    public interface IHostingPage
    {
        /// <summary>
        /// Provides access to the <see cref="Frame"/> that hosts this page's content.
        /// </summary>
        Frame ContentFrame { get; }

        /// <summary>
        /// Allows a parent page to specify an IoC container.
        /// </summary>
        IUnityContainer Container { set; }

        /// <summary>
        /// Whether this page or its content can navigate backwards.
        /// </summary>
        /// <returns></returns>
        bool CanGoBack();

        /// <summary>
        /// Navigates the content frame if possible, otherwise the current frame.
        /// </summary>
        void GoBack();
    }
}
