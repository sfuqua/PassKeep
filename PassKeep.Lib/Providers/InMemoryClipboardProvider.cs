using PassKeep.Lib.Contracts.Providers;
using System.Threading.Tasks;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A stubbed out version of a clipboard useful for unit tests.
    /// </summary>
    public class InMemoryClipboardProvider : IClipboardProvider
    {
        private string data;

        /// <summary>
        /// Sets clipboard content.
        /// </summary>
        /// <param name="data">The data to set.</param>
        public void SetContent(string data)
        {
            this.data = data;
        }

        /// <summary>
        /// Asynchronously accesses the clipboard's content as text.
        /// </summary>
        /// <returns>The clipboard's content as text if possible, else null.</returns>
        public Task<string> GetContentAsText()
        {
            return Task.FromResult(this.data);
        }

        /// <summary>
        /// Clears the clipboard.
        /// </summary>
        public void Clear()
        {
            this.data = null;
        }
    }
}
