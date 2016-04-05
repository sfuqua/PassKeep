using PassKeep.Contracts.Models;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// Barebones implementation of <see cref="IDatabaseCandidate"/>. Only simple properties 
    /// are defined, methods will throw <see cref="NotImplementedException"/>.
    /// </summary>
    public sealed class MockDatabaseCandidate : BindableBase, IDatabaseCandidate
    {
        public string CannotRememberText
        {
            get;
            set;
        }
        
        public string FileName
        {
            get;
            set;
        }

        public DateTimeOffset? LastModified
        {
            get;
            set;
        }

        public ulong Size
        {
            get;
            set;
        }

        public IStorageFile StorageItem
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task ReplaceWithAsync(IStorageFile file)
        {
            throw new NotImplementedException();
        }
    }
}
