using PassKeep.KeePassLib.SecurityTokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using PassKeep.Models;

namespace PassKeep.KeePassLib
{
    public class KdbxWriter : KdbxHandler, PassKeep.KeePassLib.IKdbxWriter
    {


        public KdbxDocument KdbxDoc
        {
            get;
            private set;
        }





        public async Task<bool> Write(IOutputStream stream, KdbxDocument document)
        {

        }








    }
}
