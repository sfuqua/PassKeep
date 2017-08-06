// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Models.DesignTime
{
    /// <summary>
    /// A mock implementation of IDatabaseCandidate for providing XAML designer data.
    /// </summary>
    public class MockDatabaseCandidate : BindableBase, IDatabaseCandidate
    {
        /// <summary>
        /// The filename to display.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// The last modified date to display.
        /// </summary>
        public DateTimeOffset? LastModified
        {
            get;
            set;
        }

        /// <summary>
        /// The file size to display.
        /// </summary>
        public ulong Size
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the app controls this file.
        /// </summary>
        public bool IsAppOwned
        {
            get;
            set;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        public Task<IRandomAccessStream> GetRandomReadAccessStreamAsync()
        {
            return null;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task ReplaceWithAsync(IStorageFile file)
        {
            return Task.CompletedTask;
        }


        public ITestableFile File
        {
            get { return null; }
        }

        public string CannotRememberText
        {
            get
            {
                return null;
            }
        }
    }
}
