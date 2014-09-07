﻿using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Windows.UI;

namespace PassKeep.Models.DesignTime
{
    public class MockGroup : IKeePassGroup
    {
        public MockGroup()
        {
            this.Children = new ObservableCollection<IKeePassNode>();
            this.IconID = KdbxGroup.DefaultIconId;
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

        public bool? EnableSearching
        {
            get;
            set;
        }

        public bool IsExpanded
        {
            get;
            set;
        }

        public ObservableCollection<IKeePassNode> Children
        {
            get;
            set;
        }

        public bool HasDescendant(IKeePassNode node)
        {
            throw new NotImplementedException();
        }

        public string DefaultAutoTypeSequence
        {
            get;
            set;
        }

        public bool? EnableAutoType
        {
            get;
            set;
        }

        public KeePassUuid LastTopVisibleEntry
        {
            get;
            set;
        }

        public bool IsSearchingPermitted()
        {
            throw new NotImplementedException();
        }

        public void SyncTo(IKeePassGroup template, bool updateModificationTime = true)
        {
            throw new NotImplementedException();
        }

        public IKeePassGroup Clone()
        {
            throw new NotImplementedException();
        }


        public IKeePassTimes Times
        {
            get;
            set;
        }
    }
}