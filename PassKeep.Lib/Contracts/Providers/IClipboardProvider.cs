using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Abstracts the Windows clipboard.
    /// </summary>
    public interface IClipboardProvider
    {
        /// <summary>
        /// Sets clipboard content.
        /// </summary>
        /// <param name="data">The data to set.</param>
        void SetContent(string data);

        /// <summary>
        /// Asynchronously accesses the clipboard's content as text.
        /// </summary>
        /// <returns>The clipboard's content as text if possible, else null.</returns>
        Task<string> GetContentAsText();

        /// <summary>
        /// Clears the clipboard.
        /// </summary>
        void Clear();
    }
}
