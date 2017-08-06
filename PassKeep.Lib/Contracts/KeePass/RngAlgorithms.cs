﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Stream cipher to use for generating random numbers.
    /// </summary>
    public enum RngAlgorithm : int
    {
        ArcFourVariant = 1,
        Salsa20 = 2,
        ChaCha20 = 3
    }
}