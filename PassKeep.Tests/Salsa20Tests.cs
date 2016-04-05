using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.KeePass.Rng;

namespace PassKeep.Tests
{
    // These test vectors are taken from the Salsa20 spec.
    // http://cr.yp.to/snuffle/spec.pdf

    [TestClass]
    public class Salsa20Tests
    {
        [TestMethod]
        public void QuarterRoundZeros()
        {
            TestQuarterRound(
                new uint[] { 0, 0, 0, 0 },
                new uint[] { 0, 0, 0, 0 }
            );
        }

        [TestMethod]
        public void QuarterRound1000()
        {
            TestQuarterRound(
                new uint[] { 1, 0, 0, 0 },
                new uint[] { 0x8008145, 0x80, 0x10200, 0x20500000 }
            );
        }

        [TestMethod]
        public void QuarterRound0100()
        {
            TestQuarterRound(
                new uint[] { 0, 1, 0, 0 },
                new uint[] { 0x88000100, 0x1, 0x200, 0x402000 }
            );
        }

        [TestMethod]
        public void QuarterRound0010() {
            TestQuarterRound(
                new uint[] { 0, 0, 1, 0 },
                new uint[] { 0x80040000, 0, 0x1, 0x2000 }
            );
        }

        [TestMethod]
        public void QuarterRound0001() {
            TestQuarterRound(
                new uint[] { 0, 0, 0, 1 },
                new uint[] { 0x48044, 0x80, 0x10000, 0x20100001 }
            );
        }

        [TestMethod]
        public void QuarterRoundArbitrary1() {
            TestQuarterRound(
                new uint[] { 0xe7e8c006, 0xc4f9417d, 0x6479b4b2, 0x68c67137 },
                new uint[] { 0xe876d72b, 0x9361dfd5, 0xf1460244, 0x948541a3 }
            );
        }

        [TestMethod]
        public void QuarterRoundArbitrary2() {
            TestQuarterRound(
                new uint[] { 0xd3917c5b, 0x55f1c407, 0x52a58a7a, 0x8f887a3b },
                new uint[] { 0x3e2f308c, 0xd90a8f36, 0x6ab2a923, 0x2883524c }
            );
        }

        [TestMethod]
        public void RowRound1000s(){
            TestRowRound(
                new uint[] 
                {
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0
                },
                new uint[]
                {
                    0x08008145,0x00000080,0x00010200,0x20500000, 
                    0x20100001,0x00048044,0x00000080,0x00010000,
                    0x00000001,0x00002000,0x80040000,0x00000000,
                    0x00000001,0x00000200,0x00402000,0x88000100
                }
            );
        }

        [TestMethod]
        public void RowRoundArbitrary()
        {
            TestRowRound(
                new uint[]
                {
                    0x08521bd6,0x1fe88837,0xbb2aa576,0x3aa26365,
                    0xc54c6a5b,0x2fc74c2f,0x6dd39cc3,0xda0a64f6,
                    0x90a2f23d,0x067f95a6,0x06b35f61,0x41e4732e,
                    0xe859c100,0xea4d84b7,0x0f619bff,0xbc6e965a
                },
                new uint[]
                {
                    0xa890d39d,0x65d71596,0xe9487daa,0xc8ca6a86,
                    0x949d2192,0x764b7754,0xe408d9b9,0x7a41b4d1, 
                    0x3402e183,0x3c3af432,0x50669f96,0xd89ef0a8, 
                    0x0040ede5,0xb545fbce,0xd257ed4f,0x1818882d
                }
            );
        }

        [TestMethod]
        public void ColumnRound1000s() {
            TestColumnRound(
                new uint[]
                {
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0
                },
                new uint[]
                {
                    0x10090288,0x00000000,0x00000000,0x00000000,
                    0x00000101,0x00000000,0x00000000,0x00000000, 
                    0x00020401,0x00000000,0x00000000,0x00000000,
                    0x40a04001,0x00000000,0x00000000,0x00000000
                }
            );
        }

        [TestMethod]
        public void ColumnRoundArbitrary(){
            TestColumnRound(
                new uint[]
                {
                    0x08521bd6,0x1fe88837,0xbb2aa576,0x3aa26365, 
                    0xc54c6a5b,0x2fc74c2f,0x6dd39cc3,0xda0a64f6,
                    0x90a2f23d,0x067f95a6,0x06b35f61,0x41e4732e,
                    0xe859c100,0xea4d84b7,0x0f619bff,0xbc6e965a
                },
                new uint[]
                {
                    0x8c9d190a,0xce8e4c90,0x1ef8e9d3,0x1326a71a, 
                    0x90a20123,0xead3c4f3,0x63a091a0,0xf0708d69, 
                    0x789b010c,0xd195a681,0xeb7d5504,0xa774135c,
                    0x481c2027,0x53a8e4b5,0x4c1f89c5,0x3f78c9c8
                }
            );
        }

        private static void TestQuarterRound(uint[] input, uint[] output)
        {
            uint[] result = Salsa20.QuarterRound(input);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(output[i], result[i]);
            }
        }

        private static void TestRowRound(uint[] input, uint[] output)
        {
            uint[] result = Salsa20.RowRound(input);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(output[i], result[i]);
            }
        }

        private static void TestColumnRound(uint[] input, uint[] output)
        {
            uint[] result = Salsa20.ColumnRound(input);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(output[i], result[i]);
            }
        }
    }
}
