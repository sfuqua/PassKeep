using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    public abstract class DatabasePersistenceViewModelTests<T> : TestClassBase
        where T : IDatabasePersistenceViewModel
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";

        protected T viewModel;
    }
}
