﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Models;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassBinAttachment : IKeePassSerializable
    {
        string FileName
        {
            get;
            set;
        }

        ProtectedBinary Data
        {
            get;
        }
    }
}
