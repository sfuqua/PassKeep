﻿using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Windows.UI;

namespace PassKeep.Models.DesignTime
{
    public class MockEntry : IKeePassEntry
    {
        public MockEntry()
        {
            this.IconID = KdbxEntry.DefaultIconId;
        }

        public IProtectedString Password
        {
            get;
            set;
        }

        public IProtectedString Url
        {
            get;
            set;
        }

        public IProtectedString UserName
        {
            get;
            set;
        }

        public string OverrideUrl
        {
            get;
            set;
        }

        public string Tags
        {
            get;
            set;
        }

        public ObservableCollection<IProtectedString> Fields
        {
            get;
            set;
        }

        public Color? ForegroundColor
        {
            get;
            set;
        }

        public Color? BackgroundColor
        {
            get;
            set;
        }

        public IKeePassAutoType AutoType
        {
            get;
            set;
        }

        public ObservableCollection<IKeePassBinary> Binaries
        {
            get;
            set;
        }

        public void SyncTo(IKeePassEntry template, bool updateModificationTime = true)
        {
            throw new NotImplementedException();
        }

        public IKeePassEntry Clone(bool preserveHistory = true)
        {
            throw new NotImplementedException();
        }

        public KeePassUuid Uuid
        {
            get;
            set;
        }

        public IProtectedString Title
        {
            get;
            set;
        }

        public IProtectedString Notes
        {
            get;
            set;
        }

        public IKeePassGroup Parent
        {
            get;
            set;
        }

        public IKeePassTimes Times
        {
            get;
            set;
        }

        public int IconID
        {
            get;
            set;
        }

        public KeePassUuid CustomIconUuid
        {
            get;
            set;
        }

        public bool HasAncestor(IKeePassGroup group)
        {
            throw new NotImplementedException();
        }

        public bool MatchesQuery(string query)
        {
            throw new NotImplementedException();
        }

        public XElement ToXml(IRandomNumberGenerator rng)
        {
            throw new NotImplementedException();
        }
    }
}