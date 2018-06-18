using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Util;
using System;
using System.Text;
using System.Xml.Linq;

namespace PassKeep.Tests
{
    [TestClass]
    public class StringTests
    {
        private IRandomNumberGenerator _rng;
        private const string KeyVal = "Key";
        private const string TextVal = "Text";
        private const string OtherTextVal = "Other";

        [TestInitialize]
        public void Setup()
        {
            byte[] random = new byte[32];
            new Random().NextBytes(random);
            this._rng = new Salsa20(random);
        }

        [TestMethod]
        public void RoundTripClear()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, false);
            Assert.AreEqual(str.ClearValue, TextVal);
        }

        [TestMethod]
        public void RoundTripProtected()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, true);
            Assert.AreEqual(str.ClearValue, TextVal);
        }

        [TestMethod]
        public void EncodedClear()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, false);
            Assert.AreEqual(str.RawValue, TextVal);
        }

        [TestMethod]
        public void EncodedProtected()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, true);
            Assert.AreNotEqual(str.RawValue, TextVal);
            Assert.IsNotNull(str.RawValue);
            Assert.AreNotEqual(str.RawValue, String.Empty);
        }

        [TestMethod]
        public void Deprotect()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, true)
                {
                    Protected = false
                };
            Assert.AreEqual(str.RawValue, TextVal);
        }

        [TestMethod]
        public void Protect()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, false)
                {
                    Protected = true
                };
            Assert.AreNotEqual(str.RawValue, TextVal);
            Assert.IsNotNull(str.RawValue);
            Assert.AreNotEqual(str.RawValue, String.Empty);
        }

        [TestMethod]
        public void ChangeClearValueUnprotected()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, false)
                {
                    ClearValue = OtherTextVal
                };
            Assert.AreEqual(str.ClearValue, OtherTextVal);
            Assert.AreEqual(str.RawValue, OtherTextVal);
        }

        [TestMethod]
        public void ChangeClearValueProtected()
        {
            KdbxString str =
                new KdbxString(KeyVal, TextVal, this._rng, true)
                {
                    ClearValue = OtherTextVal
                };
            Assert.AreEqual(str.ClearValue, OtherTextVal);
            Assert.AreNotEqual(str.RawValue, OtherTextVal);
            Assert.IsNotNull(str.RawValue);
            Assert.AreNotEqual(str.RawValue, String.Empty);
        }

        [TestMethod]
        public void SetNullUnprotected()
        {
            KdbxString str
                = new KdbxString(KeyVal, null, this._rng, false);
            Assert.IsNull(str.ClearValue);
            Assert.IsNull(str.RawValue);
        }

        [TestMethod]
        public void SetNullProtected()
        {
            KdbxString str
                = new KdbxString(KeyVal, null, this._rng, true);
            Assert.IsNull(str.ClearValue);
            Assert.IsNull(str.RawValue);
        }

        [TestMethod]
        public void SetXmlUnprotected()
        {
            XElement node = new XElement("String");
            node.Add(
                new XElement("Key", KeyVal),
                new XElement("Value", TextVal)
            );

            KdbxString str = new KdbxString(node, this._rng);
            Assert.AreEqual(str.ClearValue, TextVal);
            Assert.AreEqual(str.RawValue, TextVal);
            Assert.IsFalse(str.Protected);
            Assert.AreEqual(str.Key, KeyVal);
        }

        [TestMethod]
        public void SetXmlProtected()
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(TextVal);
            byte[] padBytes = this._rng.GetBytes((uint)clearBytes.Length);
            ByteHelper.Xor(padBytes, 0, clearBytes, 0, clearBytes.Length);
            string enc = Convert.ToBase64String(clearBytes);

            XElement node = new XElement("String");
            XElement value = new XElement("Value", enc);
            value.SetAttributeValue("Protected", "True");

            node.Add(
                new XElement("Key", KeyVal),
                value
            );

            KdbxString str = new KdbxString(node, this._rng.Clone());
            Assert.AreEqual(str.ClearValue, TextVal);
            Assert.AreNotEqual(str.RawValue, TextVal);
            Assert.IsNotNull(str.RawValue);
            Assert.AreNotEqual(str.RawValue, String.Empty);
            Assert.IsTrue(str.Protected);
            Assert.AreEqual(str.Key, KeyVal);
        }

        [TestMethod]
        public void XmlRoundTrip()
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(TextVal);
            byte[] padBytes = this._rng.GetBytes((uint)clearBytes.Length);
            ByteHelper.Xor(padBytes, 0, clearBytes, 0, clearBytes.Length);
            string enc = Convert.ToBase64String(clearBytes);

            XElement node = new XElement("String");
            XElement value = new XElement("Value", enc);
            value.SetAttributeValue("Protected", "True");

            node.Add(
                new XElement("Key", KeyVal),
                value
            );

            KdbxString str = new KdbxString(node, this._rng.Clone());
            XElement node2 = str.ToXml(this._rng.Clone(), new KdbxSerializationParameters(KdbxVersion.Unspecified));
            KdbxString str2 = new KdbxString(node2, this._rng.Clone());

            Assert.AreEqual(str.Protected, str2.Protected);
            Assert.AreEqual(str.ClearValue, str2.ClearValue);
        }
    }
}
