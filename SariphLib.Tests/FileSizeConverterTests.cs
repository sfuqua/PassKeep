using Microsoft.VisualStudio.TestTools.UnitTesting;
using SariphLib.Mvvm.Converters;

namespace SariphLib.Tests
{
    [TestClass]
    public class FileSizeConverterTests
    {
        private static readonly FileSizeConverter converter = new FileSizeConverter();

        [TestMethod]
        public void Convert0()
        {
            Assert.AreEqual("0 bytes", Convert(0));
        }

        [TestMethod]
        public void ConvertManyBytes()
        {
            Assert.AreEqual("1023 bytes", Convert(1023));
        }

        [TestMethod]
        public void ConvertOneKiB()
        {
            Assert.AreEqual("1.00 KiB", Convert(1024));
        }

        [TestMethod]
        public void ConvertSomeKiB()
        {
            // Arbitrary number to test fractions
            Assert.AreEqual("29.67 KiB", Convert(30382));
        }

        [TestMethod]
        public void ConvertManyKiB()
        {
            // Not 1023.99 due to double rounding
            Assert.AreEqual("1024.00 KiB", Convert(1024 * 1024 - 1));
        }

        [TestMethod]
        public void ConvertOneMiB()
        {
            Assert.AreEqual("1.00 MiB", Convert(1024 * 1024));
        }

        [TestMethod]
        public void ConvertOneGiB()
        {
            Assert.AreEqual("1.00 GiB", Convert(1024 * 1024 * 1024));
        }

        [TestMethod]
        public void ConvertOneTiB()
        {
            Assert.AreEqual("1.00 TiB", Convert(1024L * 1024 * 1024 * 1024));
        }

        /// <summary>
        /// Helper to fill in correct values for the converter.
        /// </summary>
        /// <param name="bytes">The size to convert.</param>
        /// <returns>The converted size.</returns>
        private string Convert(ulong bytes)
        {
            return converter.Convert(bytes, typeof(string), null, null) as string;
        }
    }
}
