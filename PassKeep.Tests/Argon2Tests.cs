using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.KeePass.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Tests
{
    [TestClass]
    public class Argon2Tests
    {
        [TestMethod]
        public void Argon2dRfcVector()
        {
            /*
             *    Memory: 32 KiB
   Iterations: 3
   Parallelism: 4 lanes
   Tag length: 32 bytes
   Password[32]: 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
   Salt[16]: 02 02 02 02 02 02 02 02 02 02 02 02 02 02 02 02
   Secret[8]: 03 03 03 03 03 03 03 03
   Associated data[12]: 04 04 04 04 04 04 04 04 04 04 04 04
   */
            byte[] password = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                password[i] = 1;
            }

            byte[] salt = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                salt[i] = 2;
            }

            byte[] secret = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                secret[i] = 3;
            }

            byte[] data = new byte[12];
            for (int i = 0; i < 12; i++)
            {
                data[i] = 4;
            }

            Argon2d engine = new Argon2d(
                password,
                salt,
                secret,
                data,
                4,
                32,
                32,
                3
            );

            byte[] output = new byte[32];
            engine.HashAsync(output);
        }

        [TestMethod]
        public void Argon2ArbitraryTest()
        {
            /*
             *    Memory: 32 KiB
   Iterations: 3
   Parallelism: 4 lanes
   Tag length: 32 bytes
   Password[32]: 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
                 01 01 01 01 01 01 01 01
   Salt[16]: 02 02 02 02 02 02 02 02 02 02 02 02 02 02 02 02
   Secret[8]: 03 03 03 03 03 03 03 03
   Associated data[12]: 04 04 04 04 04 04 04 04 04 04 04 04
   */
            byte[] password = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                password[i] = 0;
            }

            byte[] salt = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                salt[i] = 1;
            }

            Argon2d engine = new Argon2d(
                password,
                salt,
                new byte[0],
                new byte[0],
                1,
                16,
                8,
                3
            );

            byte[] output = new byte[16];
            engine.HashAsync(output);
        }
    }
}
