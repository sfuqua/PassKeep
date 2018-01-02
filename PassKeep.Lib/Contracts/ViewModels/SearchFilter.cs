// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public sealed class SearchFilter : BindableBase
    {
        private String _name;
        private int _count;
        private bool _active;

        public SearchFilter(String name, int count, bool active = false)
        {
            Name = name;
            Count = count;
            Active = active;
        }

        public override String ToString()
        {
            return Description;
        }

        public String Name
        {
            get { return this._name; }
            set { if (TrySetProperty(ref this._name, value)) OnPropertyChanged("Description"); }
        }

        public int Count
        {
            get { return this._count; }
            set { if (TrySetProperty(ref this._count, value)) OnPropertyChanged("Description"); }
        }

        public bool Active
        {
            get { return this._active; }
            set { TrySetProperty(ref this._active, value); }
        }

        public String Description
        {
            get { return String.Format("{0} ({1})", this._name, this._count); }
        }
    }
}
