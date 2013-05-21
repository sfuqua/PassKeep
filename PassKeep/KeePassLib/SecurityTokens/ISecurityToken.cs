using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassLib.SecurityTokens
{
    public interface ISecurityToken
    {
        Task<IBuffer> GetBuffer();
    }
}
