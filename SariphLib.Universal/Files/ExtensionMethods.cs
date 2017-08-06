// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

using FileAttributes = Windows.Storage.FileAttributes;

namespace SariphLib.Files
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// The file properties key used to access file attributes.
        /// </summary>
        public const string FileAttributesKey = "System.FileAttributes";

        /// <summary>
        /// Extension helper to create an <see cref="ITestableFile"/> from a <see cref="StorageFile"/>.
        /// </summary>
        /// <param name="file">The file to wrap.</param>
        /// <returns>A wrapper suitable for testing.</returns>
        public static ITestableFile AsWrapper(this StorageFile file)
        {
            return new StorageFileWrapper(file);
        }

        /// <summary>
        /// Asynchronously sets file attribute flags on this file.
        /// </summary>
        /// <param name="file">The file to modify.</param>
        /// <returns></returns>
        public static async Task ClearFileAttributesAsync(this StorageFile file, FileAttributes attributes)
        {
            string[] propertyList = new string[] { FileAttributesKey };
            IDictionary<string, object> props = await file.Properties.RetrievePropertiesAsync(propertyList)
                .AsTask().ConfigureAwait(false);

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
            IDictionary<string, object> props = await file.Properties.RetrievePropertiesAsync(propertyList)
                .AsTask().ConfigureAwait(false);

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
        /// <param name="bypassShortcut">Whether to ignore StorageFile.Attributes and directly try to open the stream.</param>
        /// <returns>Whether we can open a writable stream to the file.</returns>
        public static async Task<bool> CheckWritableAsync(this IStorageFile file, bool bypassShortcut = false)
        {
            // SURPRISE! StorageFile.Attributes is utterly worthless for checking the current state
            // of a file. We'll need to always try to just open a stream and see what happens.
            // Short-circuit fast case
            //if (!bypassShortcut && file.Attributes.HasFlag(FileAttributes.ReadOnly))
            //{
            //    return false;
            //}

            // If ReadOnly isn't set, we might still have a problem, especially on phone...
            try
            {
                using (await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
                {
                    return true;
                }
            }
            catch (Exception e)
                when (e is UnauthorizedAccessException || e is FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously returns whether we have write access to this file.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <param name="bypassShortcut">Whether to ignore StorageFile.Attributes and directly try to open the stream.</param>
        /// <returns>Whether we can open a writable stream to the file.</returns>
        public static Task<bool> CheckWritableAsync(this ITestableFile file, bool bypassShortcut = false)
        {
            return file.AsIStorageFile.CheckWritableAsync(bypassShortcut);
        }
    }
}
