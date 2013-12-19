using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxReader
    {
        Task<DecryptionResult> DecryptFile(IRandomAccessStream stream, string password, StorageFile keyfile);
        void Cancel();

        IKdbxWriter GetWriter();

        Task<KeePassError> ReadHeader(IRandomAccessStream stream);
    }
}
