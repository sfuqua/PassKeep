using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.KeePass.Crypto;
using System.Threading;
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
        public async Task Argon2dRfcVector()
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
            await engine.HashAsync(output, CancellationToken.None);

            byte[] expectedOutput = new byte[32]
            {
                0x51, 0x2b, 0x39, 0x1b, 0x6f, 0x11, 0x62, 0x97,
                0x53, 0x71, 0xd3, 0x09, 0x19, 0x73, 0x42, 0x94,
                0xf8, 0x68, 0xe3, 0xbe, 0x39, 0x84, 0xf3, 0xc1,
                0xa1, 0x3a, 0x4d, 0xb9, 0xfa, 0xbe, 0x4a, 0xcb
            };

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"out[{i}]");
            }
        }

        /// <summary>
        /// Test vector with segment lengths of 4+.
        /// </summary>
        [TestMethod]
        public async Task Argon2LongerSegments()
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
                2,
                8,
                32,
                5
            );

            byte[] output = new byte[8];
            await engine.HashAsync(output, CancellationToken.None);

            byte[] expectedOutput = new byte[8]
            {
                0xa4, 0x26, 0xcc, 0xef, 0x1b, 0x72, 0x4a, 0xaf
            };

            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"out[{i}]");
            }
        }

        /// <summary>
        /// A basic test with Parallelism = 1 over 8 Kb of memory.
        /// </summary>
        [TestMethod]
        public async Task Argon2SimpleSingleThread()
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
            await engine.HashAsync(output, CancellationToken.None);

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

        /// <summary>
        /// Test vector from the RFC over 1024 KiB with Parallelism = 8.
        /// </summary>
        [TestMethod]
        public async Task Argon2HeavyMultiThread()
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
                8,
                16,
                40960,
                8
            );

            byte[] output = new byte[16];
            await engine.HashAsync(output, CancellationToken.None);

            byte[] expectedOutput = new byte[16]
            {
                0x23, 0xb8, 0x9e, 0x2f, 0x01, 0xce, 0x60, 0xa1,
                0xd3, 0x2b, 0xe8, 0xa0, 0x92, 0x64, 0xc2, 0x80
            };

            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"out[{i}]");
            }
        }
    }
}
