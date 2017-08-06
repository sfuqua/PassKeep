// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
