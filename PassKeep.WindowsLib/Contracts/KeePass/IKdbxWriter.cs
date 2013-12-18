using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.SecurityTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxWriter
    {
        KeyFile KeyFile { get; set; }
        string Password { get; set; }
        Task<bool> Write(StorageFile file, KdbxDocument document);
        Task<bool> Write(IOutputStream stream, KdbxDocument document);
        void Cancel();
    }
}
