// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace PassKeep.Lib.Providers
{
    public class WindowsClipboardProvider : IClipboardProvider
    {
        /// <summary>
        /// Sets clipboard content.
        /// </summary>
        /// <param name="data">The data to set.</param>
        public void SetContent(string data)
        {
            if (String.IsNullOrEmpty(data))
            {
                Clipboard.SetContent(null);
            }
            else
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(data);
                Clipboard.SetContent(dataPackage);
            }
        }

        /// <summary>
        /// Asynchronously accesses the clipboard's content as text.
        /// </summary>
        /// <returns>The clipboard's content as text if possible, else null.</returns>
        public async Task<string> GetContentAsText()
        {
            DataPackageView dataView;
            try
            { 
                dataView = Clipboard.GetContent();
            }
            catch(Exception)
            {
                return null;
            }

            return await dataView.GetTextAsync();
        }

        /// <summary>
        /// Clears the clipboard.
        /// </summary>
        public void Clear()
        {
            Clipboard.Clear();
        }
    }
}
