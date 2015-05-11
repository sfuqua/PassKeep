using Windows.UI.Xaml.Controls;

namespace PassKeep.Framework
{
    /// <summary>
    /// A page that hosts other content and provides access to the <see cref="Frame"/>
    /// it uses.
    /// </summary>
    public interface INestingPage
    {
        /// <summary>
        /// Provides access to the <see cref="Frame"/> that hosts this page's content.
        /// </summary>
        Frame ContentFrame { get; }
    }
}
