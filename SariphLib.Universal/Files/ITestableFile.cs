using Windows.Storage;

namespace SariphLib.Files
{
    /// <summary>
    /// Helper interface that aggregates the interfaces that <see cref="StorageFile"/>
    /// implements. Methods that take a storage file should use this instead in conjunction
    /// with <see cref="StorageFileWrapper"/>.
    /// </summary>
    public interface ITestableFile : IStorageFile, IStorageFile2, IStorageItem, IStorageItem2
    {
    }
}
