using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.KeePassLib
{
    /// <summary>
    /// Stream cipher to use for generating random numbers.
    /// </summary>
    public enum RngAlgorithm : int
    {
        ArcFourVariant = 1,
        Salsa20 = 2,
        ChaCha20 = 3
    }
}
