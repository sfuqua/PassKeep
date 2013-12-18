using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface ISecurityToken
    {
        Task<IBuffer> GetBuffer();
    }
}
