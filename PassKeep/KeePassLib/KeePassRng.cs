using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using PassKeep.KeePassLib;

namespace PassKeep.KeePassLib
{
    public abstract class KeePassRng
    {
        protected internal byte[] Seed
        {
            get;
            protected set;
        }

        public KeePassRng(byte[] seed)
        {
            this.Seed = seed;
        }

        public abstract byte[] GetBytes(uint numBytes);
        public abstract KeePassRng Clone();
        public abstract KdbxHandler.RngAlgorithm Algorithm { get; }
    }
}
