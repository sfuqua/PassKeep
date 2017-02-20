using PassKeep.Lib.Models;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassBinAttachment : IKeePassSerializable
    {
        string FileName
        {
            get;
            set;
        }

        ProtectedBinary Data
        {
            get;
        }
    }
}
