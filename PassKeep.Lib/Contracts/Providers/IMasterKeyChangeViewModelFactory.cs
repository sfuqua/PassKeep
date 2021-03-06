﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface IMasterKeyChangeViewModelFactory
    {
        IMasterKeyViewModel Assemble(KdbxDocument document, IDatabasePersistenceService persistenceService, ITestableFile databaseFile);
    }
}
