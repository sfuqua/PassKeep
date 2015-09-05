﻿using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Util;
using System;
using System.Text;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxString : KdbxPart, IProtectedString
    {
        private object syncRoot = new object();

        public static KdbxString Empty
        {
            get;
            private set;
        }

        static KdbxString()
        {
            Empty = new KdbxString();
            Empty._rng = null;
            Empty.Protected = false;
            Empty.Key = null;
            Empty.ClearValue = null;
        }

        public static string RootName
        {
            get { return "String"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private string _key;
        public string Key
        {
            get { return _key; }
            set { TrySetProperty(ref _key, value); }
        }

        private string _rawValue;
        public string RawValue
        {
            get
            {
                lock (syncRoot)
                {
                    return _rawValue;
                }
            }
            private set
            {
                lock (syncRoot)
                {
                    TrySetProperty(ref _rawValue, value);
                }
            }
        }

        private bool _protected;
        public bool Protected
        {
            get
            {
                lock (syncRoot)
                {
                    return _protected;
                }
            }
            set
            {
                lock (syncRoot)
                {
                    if (Protected == value)
                    {
                        return;
                    }

                    if (value)
                    {
                        // We're now protected. Set the XOR key and update RawValue.
                        if (ClearValue != null)
                        {
                            _xorKey = _rng.GetBytes((uint)Encoding.UTF8.GetBytes(ClearValue).Length);
                            RawValue = getEncrypted(ClearValue, _xorKey);
                        }
                    }
                    else
                    {
                        // We're now clear. Get rid of the key and update RawValue.
                        RawValue = ClearValue;
                        _xorKey = null;
                    }

                    TrySetProperty(ref _protected, value);
                }
            }
        }

        private byte[] _xorKey;
        public string ClearValue
        {
            get
            {
                lock (syncRoot)
                {
                    if (!Protected)
                    {
                        return RawValue;
                    }
                    return getDecrypted(RawValue, _xorKey);
                }
            }
            set
            {
                lock (syncRoot)
                {
                    if (!Protected)
                    {
                        _xorKey = null;
                        RawValue = value;
                    }
                    else if (value == null)
                    {
                        RawValue = null;
                    }
                    else
                    {
                        _xorKey = _rng.GetBytes((uint)Encoding.UTF8.GetBytes(value).Length);
                        RawValue = getEncrypted(value, _xorKey);
                    }

                    OnPropertyChanged();
                }
            }
        }

        private IRandomNumberGenerator _rng;
        private KdbxString() { }

        public KdbxString(XElement xml, IRandomNumberGenerator rng)
            : base(xml)
        {
            _rng = rng;

            XElement valueNode = GetNode("Value");
            XAttribute protectedAttr = valueNode.Attribute("Protected");
            if (protectedAttr != null && protectedAttr.Value == "True")
            {
                Protected = true;
            }
            else
            {
                Protected = false;
            }

            Key = GetString("Key");
            RawValue = valueNode.Value;
            if (Protected)
            {
                _xorKey = rng.GetBytes((uint)getBytes(RawValue).Length);
            }
        }

        public KdbxString(string key, string clearValue, IRandomNumberGenerator rng, bool protect = false)
        {
            _rng = rng;
            Protected = protect;
            Key = key;
            ClearValue = clearValue;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            xml.Add(new XElement("Key", Key));

            XElement valueElement = new XElement("Value");
            if (!string.IsNullOrEmpty(RawValue))
            {
                string value = (Protected ?
                    getEncrypted(ClearValue, rng.GetBytes((uint)_xorKey.Length)) :
                    ClearValue
                    );
                valueElement.SetValue(value);
            }
            xml.Add(valueElement);

            if (Protected)
            {
                valueElement.SetAttributeValue("Protected", "True");
            }
        }

        private static byte[] getBytes(string value)
        {
            return Convert.FromBase64String(value);
        }

        private static string getString(byte[] value)
        {
            return Encoding.UTF8.GetString(value, 0, value.Length);
        }

        private static string getEncrypted(string clear, byte[] key)
        {
            if (clear == null)
            {
                return null;
            }

            byte[] clearBytes = Encoding.UTF8.GetBytes(clear);
            ByteHelper.Xor(key, 0, clearBytes, 0, clearBytes.Length);
            return Convert.ToBase64String(clearBytes);
        }

        private static string getDecrypted(string cipher, byte[] key)
        {
            if (cipher == null)
            {
                return null;
            }

            byte[] cipherBytes = getBytes(cipher);
            ByteHelper.Xor(key, 0, cipherBytes, 0, cipherBytes.Length);
            return getString(cipherBytes);
        }

        public IProtectedString Clone()
        {
            KdbxString clone = new KdbxString();
            if (_rng != null)
            {
                clone._rng = this._rng.Clone();
            }
            else
            {
                clone._rng = null;
            }
            clone.Protected = this.Protected;
            clone.Key = this.Key;
            clone.ClearValue = this.ClearValue;
            return clone;
        }

        public override bool Equals(object obj)
        {
            KdbxString other = obj as KdbxString;
            if (other == null)
            {
                return false;
            }

            return this.Protected == other.Protected && 
                this.ClearValue == other.ClearValue &&
                this.Key == other.Key;
        }

        public override string ToString()
        {
            return String.Format("KdbxString<{0}>", this.ClearValue);
        }

        public int CompareTo(IProtectedString other)
        {
            // If the other string is null, this instance is greater.
            if (other == null)
            {
                return 1;
            }

            // Compare by ClearValue strings
            return this.ClearValue.CompareTo(other.ClearValue);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}