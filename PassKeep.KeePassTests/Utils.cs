using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PassKeep.KeePassTests
{
    public static class Utils
    {
        public static async Task<StorageFile> GetPackagedFile(string folder, string file)
        {
            StorageFolder installFolder = Package.Current.InstalledLocation;

            if (folder != null)
            {
                StorageFolder subFolder = await installFolder.GetFolderAsync(folder);
                return await subFolder.GetFileAsync(file);
            }
            else
            {
                return await installFolder.GetFileAsync(file);
            }
        }

        public static async Task<StorageFolder> GetWorkFolder()
        {
            StorageFolder workFolder = await
                ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Work", CreationCollisionOption.ReplaceExisting);
            return workFolder;
        }
    }
}
