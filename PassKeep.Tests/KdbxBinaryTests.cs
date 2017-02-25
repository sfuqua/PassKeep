using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Models;
using PassKeep.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for <see cref="KdbxBinaries"/>, <see cref="KdbxBinary"/>,
    /// <see cref="KdbxBinAttachment"/>, and <see cref="ProtectedBinary"/>.
    /// </summary>
    [TestClass]
    public class KdbxBinaryTests : TestClassBase
    {
        private const string TestString = "abc123";
        private const string Base64Uncompressed = "YWJjMTIz";
        private const string Base64Gzip = "H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/IrLJdHfv3v8DXLsCzwYAAAA=";
        private const string Base64Gzip2 = "H4sIAAAAAAAAC0tMSjY0MgYAXLsCzwYAAAA=";
        private static readonly byte[] ExpectedBytes = Encoding.UTF8.GetBytes(TestString);

        public override TestContext TestContext
        {
            get; set;
        }

        /// <summary>
        /// Tests deserializing a missing attribute is handled cleanly.
        /// </summary>
        /// <param name="version"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, CompressionAlgorithm.None),
            DataRow(KdbxVersion.Four, CompressionAlgorithm.GZip)]
        public void DeserializeMissingId(KdbxVersion version, CompressionAlgorithm compression)
        {
            Assert.ThrowsException<KdbxParseException>(
                () =>
                {
                    new KdbxBinary(
                        new XElement(KdbxBinary.RootName, Base64Uncompressed),
                        new KdbxSerializationParameters(version) { Compression = compression }
                    );
                }
            );
        }

        /// <summary>
        /// Tests missing/empty data is deserialized correctly.
        /// </summary>
        /// <param name="version"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, CompressionAlgorithm.None),
            DataRow(KdbxVersion.Four, CompressionAlgorithm.GZip)]
        public void DeserializeNoValue(KdbxVersion version, CompressionAlgorithm compression)
        {
            KdbxBinary binary = new KdbxBinary(
                new XElement(KdbxBinary.RootName, "",
                    new XAttribute("ID", 1)
                ),
                new KdbxSerializationParameters(version) { Compression = compression }
            );

            Assert.AreEqual(1, binary.Id);
            Assert.AreEqual(0, binary.BinaryData.GetClearData().Length);
        }

        /// <summary>
        /// Tests missing/empty data is deserialized correctly.
        /// </summary>
        /// <param name="version"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, CompressionAlgorithm.None),
            DataRow(KdbxVersion.Four, CompressionAlgorithm.GZip)]
        public void DeserializeNoValueCompressed(KdbxVersion version, CompressionAlgorithm compression)
        {
            KdbxBinary binary = new KdbxBinary(
                new XElement(KdbxBinary.RootName, "",
                    new XAttribute("ID", 1),
                    new XAttribute("Compressed", "True")
                ),
                new KdbxSerializationParameters(version) { Compression = compression }
            );

            Assert.AreEqual(1, binary.Id);
            Assert.AreEqual(0, binary.BinaryData.GetClearData().Length);
        }

        /// <summary>
        /// Tests valid uncompressed data is deserialized correctly.
        /// </summary>
        /// <param name="version"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, CompressionAlgorithm.None),
            DataRow(KdbxVersion.Four, CompressionAlgorithm.GZip)]
        public void DeserializeUncompressed(KdbxVersion version, CompressionAlgorithm compression)
        {
            KdbxBinary binary = new KdbxBinary(
                new XElement(KdbxBinary.RootName, Base64Uncompressed,
                    new XAttribute("ID", 1)
                ),
                new KdbxSerializationParameters(version) { Compression = compression }
            );

            Assert.AreEqual(1, binary.Id);
            ValidateBytes(binary.BinaryData.GetClearData());
        }

        /// <summary>
        /// Tests valid compressed data is deserialized correctly.
        /// </summary>
        /// <param name="version"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, CompressionAlgorithm.None),
            DataRow(KdbxVersion.Four, CompressionAlgorithm.GZip)]
        public void DeserializeCompressed(KdbxVersion version, CompressionAlgorithm compression)
        {
            KdbxBinary binary = new KdbxBinary(
                new XElement(KdbxBinary.RootName, Base64Gzip,
                    new XAttribute("ID", 1),
                    new XAttribute("Compressed", "True")
                ),
                new KdbxSerializationParameters(version) { Compression = compression }
            );

            Assert.AreEqual(1, binary.Id);
            ValidateBytes(binary.BinaryData.GetClearData());
        }

        /// <summary>
        /// Tests serializing a binary containing known data, without compression.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="protect"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, false), DataRow(KdbxVersion.Four, true)]
        public void SerializeUncompressed(KdbxVersion version, bool protect)
        {
            ProtectedBinary bin = new ProtectedBinary(ExpectedBytes, protect);
            KdbxBinary binary = new KdbxBinary(1, bin);
            XElement serialized = binary.ToXml(
                new MockRng(),
                new KdbxSerializationParameters(version) { Compression = CompressionAlgorithm.None }
            );

            Assert.AreEqual(binary.Id.ToString(), serialized.Attribute("ID").Value, "ID should serialize properly");
            Assert.IsNull(serialized.Attribute("Compressed"), "Compressed attribute should not be set");
            Assert.AreEqual(Base64Uncompressed, serialized.Value, "XML value should serialize properly");
        }

        /// <summary>
        /// Tests serializing a binary containing no data, without compression.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="protect"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, false), DataRow(KdbxVersion.Four, true)]
        public void SerializeNoValue(KdbxVersion version, bool protect)
        {
            ProtectedBinary bin = new ProtectedBinary(new byte[0], protect);
            KdbxBinary binary = new KdbxBinary(1, bin);
            XElement serialized = binary.ToXml(
                new MockRng(),
                new KdbxSerializationParameters(version) { Compression = CompressionAlgorithm.None }
            );

            Assert.AreEqual(binary.Id.ToString(), serialized.Attribute("ID").Value, "ID should serialize properly");
            Assert.IsNull(serialized.Attribute("Compressed"), "Compressed attribute should not be set");
            Assert.AreEqual("", serialized.Value, "XML value should serialize properly");
        }

        /// <summary>
        /// Tests serializing a binary containing known data, with compression.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="protect"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, false), DataRow(KdbxVersion.Four, true)]
        public void SerializeCompressed(KdbxVersion version, bool protect)
        {
            ProtectedBinary bin = new ProtectedBinary(ExpectedBytes, protect);
            KdbxBinary binary = new KdbxBinary(1, bin);
            XElement serialized = binary.ToXml(
                new MockRng(),
                new KdbxSerializationParameters(version) { Compression = CompressionAlgorithm.GZip }
            );

            Assert.AreEqual(binary.Id.ToString(), serialized.Attribute("ID").Value, "ID should serialize properly");
            Assert.AreEqual("True", serialized.Attribute("Compressed").Value, "Compressed attribute should not be set");
            Assert.AreEqual(Base64Gzip2, serialized.Value, "XML value should serialize properly");
        }

        /// <summary>
        /// Tests serializing a binary containing no data, with compression.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="protect"></param>
        [DataTestMethod]
        [DataRow(KdbxVersion.Three, false), DataRow(KdbxVersion.Four, true)]
        public void SerializeNoValueCompressed(KdbxVersion version, bool protect)
        {
            ProtectedBinary bin = new ProtectedBinary(new byte[0], protect);
            KdbxBinary binary = new KdbxBinary(1, bin);
            XElement serialized = binary.ToXml(
                new MockRng(),
                new KdbxSerializationParameters(version) { Compression = CompressionAlgorithm.GZip }
            );

            Assert.AreEqual(binary.Id.ToString(), serialized.Attribute("ID").Value, "ID should serialize properly");
            Assert.AreEqual("True", serialized.Attribute("Compressed").Value, "Compressed attribute should not be set");
            Assert.AreEqual("", serialized.Value, "XML value should serialize properly");
        }

        /// <summary>
        /// Tests that protected data is properly encrypted in-memory.
        /// </summary>
        [TestMethod]
        public void DataProtection()
        {
            ProtectedBinary bin = new ProtectedBinary(ExpectedBytes, true);
            Assert.IsFalse(bin.GetRawData().SequenceEqual(ExpectedBytes), "Raw data should not be the same as clear data");
            Assert.IsTrue(bin.GetClearData().SequenceEqual(ExpectedBytes), "Clear data should be the same as original");

            // Do it again to ensure the cipher is re-seeded
            ProtectedBinary bin2 = new ProtectedBinary(ExpectedBytes, true);
            Assert.IsFalse(bin.GetRawData().SequenceEqual(bin2.GetRawData()), "Raw data should differ binary-to-binary");
            Assert.IsTrue(bin2.GetClearData().SequenceEqual(ExpectedBytes), "Clear data should be the same as original, second time");

            ProtectedBinary clearBin = new ProtectedBinary(ExpectedBytes, false);
            Assert.IsTrue(clearBin.GetRawData().SequenceEqual(ExpectedBytes), "Unencrypted data should be exposed as-is");
            Assert.IsTrue(clearBin.GetClearData().SequenceEqual(ExpectedBytes), "Unencrypted data should not be encrypted accidentally");
        }

        /// <summary>
        /// Asserts that the specified byte array is equal to
        /// <see cref="ExpectedBytes"/>.
        /// </summary>
        /// <param name="buffer"></param>
        private void ValidateBytes(byte[] buffer)
        {
            Assert.AreEqual(ExpectedBytes.Length, buffer.Length, "Buffer lengths should match");
            for (int i = 0; i < ExpectedBytes.Length; i++)
            {
                Assert.AreEqual(ExpectedBytes[i], buffer[i], $"bytes[{i}] should match");
            }
        }
    }
}
