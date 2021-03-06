﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxAutoType : KdbxPart, IKeePassAutoType
    {
        public static string RootName
        {
            get { return "AutoType"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement original;
        public KdbxAutoType(XElement xml)
            : base(xml)
        {
            this.original = xml;
        }

        public override void PopulateChildren(XElement element, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        { }

        public override bool Equals(object obj)
        {
            KdbxAutoType other = obj as KdbxAutoType;
            if (other == null)
            {
                return false;
            }

            return XElement.DeepEquals(this.original, other.original);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
