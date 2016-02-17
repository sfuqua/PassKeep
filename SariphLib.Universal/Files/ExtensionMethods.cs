using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SariphLib.Files
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// The file properties key used to access file attributes.
        /// </summary>
        public const string FileAttributesKey = "System.FileAttributes";

        /// <summary>
        /// Asynchronously sets file attribute flags on this file.
        /// </summary>
        /// <param name="file">The file to modify.</param>
        /// <returns></returns>
        public static async Task ClearFileAttributesAsync(this StorageFile file, FileAttributes attributes)
        {
            string[] propertyList = new string[] { FileAttributesKey };
            IDictionary<string, object> props = await file.Properties.RetrievePropertiesAsync(propertyList);

            if (props != null)
            {
                props[FileAttributesKey] = (uint)props[FileAttributesKey] & ~((uint)attributes);
            }
            else
            {
                props = new PropertySet();
            }

            await file.Properties.SavePropertiesAsync(props);
        }

        /// <summary>
        /// Asynchronously sets file attribute flags on this file.
        /// </summary>
        /// <param name="file">The file to modify.</param>
        /// <returns></returns>
        public static async Task SetFileAttributesAsync(this StorageFile file, FileAttributes attributes)
        {
            string[] propertyList = new string[] { FileAttributesKey };
            IDictionary<string, object> props = await file.Properties.RetrievePropertiesAsync(propertyList);

            if (props != null)
            {
                props[FileAttributesKey] = (uint)props[FileAttributesKey] | (uint)attributes;
            }
            else
            {
                props = new PropertySet
                {
                    { FileAttributesKey, (uint)attributes }
                };
            }

            await file.Properties.SavePropertiesAsync(props);
        }

        /// <summary>
        /// Asynchronously sets the read-only attribute of this file.
        /// </summary>
        /// <param name="file">The file to make read-only.</param>
        /// <returns></returns>
        public static async Task SetReadOnlyAsync(this StorageFile file)
        {
            await file.SetFileAttributesAsync(FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Asynchronously returns whether we have write access to this file.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <returns>Whether we can open a writable stream to the file.</returns>
        public static Task<bool> CheckWritableAsync(this IStorageFile file)
        {
            // Short-circuit fast case
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                return Task.FromResult(false);
            }

            // If ReadOnly isn't set, we might still have a problem, especially on phone...
            return Task.Run(() =>
                file.OpenAsync(FileAccessMode.ReadWrite).AsTask()
                .ContinueWith(
                    (openTask) =>
                    {
                        if (openTask.Exception != null)
                        {
                            if (openTask.Exception.InnerException is UnauthorizedAccessException)
                            {
                                return false;
                            }
                            else
                            {
                                throw openTask.Exception;
                            }
                        }

                        openTask.Result.Dispose();
                        return true;
                    }
                )
            );
        }
    }
}
