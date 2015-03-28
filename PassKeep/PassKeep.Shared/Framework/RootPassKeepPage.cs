using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep.
    /// </summary>
    public abstract class RootPassKeepPage : Page
    {
        private static readonly Action NoOp = () => { };

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        /// <param name="cancelledCallback">Callback to invoke if the user pressed 'cancel'.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback, Action cancelledCallback)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Not all databases end in .kdbx
            picker.FileTypeFilter.Add("*");

            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {
                Dbg.Trace("User cancelled the file picker.");
                cancelledCallback();
            }
            else
            {
                Dbg.Trace("User selected a file via the picker.");
                gotFileCallback(pickedFile);
            }
        }

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback)
        {
            await PickFile(gotFileCallback, PassKeepPage.NoOp);
        }
    }
}
