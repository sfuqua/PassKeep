using PassKeep.Lib.Contracts.KeePass;
using System;

namespace PassKeep.Lib.KeePass
{
    public class KdbxParseException : Exception
    {
        public KeePassError Error
        {
            get;
            private set;
        }

        public KdbxParseException(KeePassError error)
        {
            Error = error;
        }

        public KdbxParseException(KdbxParseError error)
            : this(new KeePassError(error))
        { }
    }
}
