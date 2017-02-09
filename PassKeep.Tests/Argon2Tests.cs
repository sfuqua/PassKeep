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
        /// <summary>
        /// Test vector from the RFC over 32 KiB with Parallelism = 4.
        /// </summary>
        [TestMethod]
        public void Argon2dRfcVector()
        {
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

            byte[] expectedOutput = new byte[32]
            {
                0xc8, 0x14, 0xd9, 0xd1, 0xdc, 0x7f, 0x37, 0xaa,
                0x13, 0xf0, 0xd7, 0x7f, 0x24, 0x94, 0xbd, 0xa1,
                0xc8, 0xde, 0x6b, 0x01, 0x6d, 0xd3, 0x88, 0xd2,
                0x99, 0x52, 0xa4, 0xc4, 0x67, 0x2b, 0x6c, 0xe8,
            };

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"out[{i}]");
            }
        }

        /// <summary>
        /// A basic test with Parallelism = 1 over 8 Kb of memory.
        /// </summary>
        [TestMethod]
        public void Argon2SimpleSingleThread()
        {
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

            byte[] expectedOutput = new byte[16]
            {
                0x6d, 0x1a, 0x2c, 0x5f, 0x65, 0x4a, 0x27, 0x7e,
                0x03, 0x9e, 0xad, 0xe2, 0x03, 0x68, 0x21, 0x61
            };

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"out[{i}]");
            }
        }
    }
}
