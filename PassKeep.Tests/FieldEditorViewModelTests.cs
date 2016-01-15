using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Core;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Tests
{
    [TestClass]
    public class FieldEditorViewModelTests : TestClassBase
    {
        private const string ExistingFieldKey = "TestField";
        private const string ExistingFieldValue = "Some value";
        private const bool ExistingFieldProtected = false;

        private const string EditedFieldKey = "TestField2";
        private const string EditedFieldValue = "Another value";
        private const bool EditedFieldProtected = false;

        private const string NewFieldKey = "TestField3";
        private const string NewFieldValue = "Some other value";
        private const bool NewFieldProtected = true;

        private readonly string[] ReservedKeys = { "UserName", "Password", "Title", "Notes", "URL" };

        private IFieldEditorViewModel viewModel;
        private IProtectedString originalString;
        private IKeePassEntry parentEntry;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public void Initialize()
        {
            IRandomNumberGenerator rng = new Salsa20(new byte[32]);

            IResourceProvider resourceProvider = new MockResourceProvider();

            this.parentEntry = new MockEntry();
            this.parentEntry.Fields.Add(
                new KdbxString(ExistingFieldKey, ExistingFieldValue, rng, ExistingFieldProtected)
            );

            MethodInfo testMethod = this.GetType().GetRuntimeMethod(
                this.TestContext.TestName, new Type[0]
            );

            DetailsForAttribute specAttr = testMethod.GetCustomAttribute<DetailsForAttribute>();
            if (specAttr == null)
            {
                return;
            }
            else
            {
                if (specAttr.IsNew)
                {
                    this.viewModel = new FieldEditorViewModel(rng, resourceProvider);
                }
                else
                {
                    this.originalString = new KdbxString(EditedFieldKey, EditedFieldValue, rng, EditedFieldProtected);
                    this.parentEntry.Fields.Add(this.originalString);

                    this.viewModel = new FieldEditorViewModel(this.originalString, resourceProvider);
                }
            }
        }

        [TestMethod, DetailsFor(isNew: true)]
        public void FieldEditorViewModel_NewFieldDefaults()
        {
            Assert.AreEqual(((FieldEditorViewModel)this.viewModel).LocalizedMissingKey, this.viewModel.ValidationError, "A new field should not be immediately save-able");
            Assert.IsNotNull(this.viewModel.WorkingCopy, "A new field should have a valid WorkingCopy");
            Assert.IsFalse(this.viewModel.WorkingCopy.Protected, "A new field should default to unprotected");
            Assert.AreEqual(String.Empty, this.viewModel.WorkingCopy.Key, "A new field should have an empty (non-null) key");
            Assert.AreEqual(String.Empty, this.viewModel.WorkingCopy.ClearValue, "A new field should have an empty (non-null) value");

            Assert.IsNotNull(this.viewModel.CommitCommand, "A new field should have a valid CommitCommand");
            Assert.IsFalse(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A new field should not be immediately save-able");
        }

        [TestMethod, DetailsFor(isNew: false)]
        public void FieldEditorViewModel_ExistingFieldDefaults()
        {
            Assert.AreEqual(String.Empty, this.viewModel.ValidationError, "An existing field should be valid");
            Assert.IsNotNull(this.viewModel.WorkingCopy, "An existing field should have a valid WorkingCopy");
            Assert.AreNotSame(this.originalString, this.viewModel.WorkingCopy, "The existing field's WorkingCopy should not be ref-equal to the original field");
            Assert.AreEqual(this.originalString, this.viewModel.WorkingCopy, "The existing field's WorkingCopy should be .Equals to the original field");

            Assert.IsNotNull(this.viewModel.CommitCommand, "An existing field should have a valid CommitCommand");
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "An existing field should be immediately save-able");
        }

        [TestMethod, DetailsFor(isNew: true)]
        public void FieldEditorViewModel_NewFieldCommit()
        {
            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.NewFieldKey;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A new field should be save-able after a key is added");

            this.viewModel.WorkingCopy.ClearValue = FieldEditorViewModelTests.NewFieldValue;
            this.viewModel.WorkingCopy.Protected = FieldEditorViewModelTests.NewFieldProtected;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A new field should still be save-able after editing properties");

            this.viewModel.CommitCommand.Execute(this.parentEntry);
            Assert.AreEqual(2, this.parentEntry.Fields.Count, "Saving a new field should increase the number of fields of the entry");
            Assert.AreEqual(this.viewModel.WorkingCopy, this.parentEntry.Fields[1], "The new field in the entry's collection should be .Equals to the WorkingCopy of the ViewModel");

            Assert.AreEqual(this.viewModel.WorkingCopy.Key, FieldEditorViewModelTests.NewFieldKey, "The WorkingCopy should have expected values after the commit");
            Assert.AreEqual(this.viewModel.WorkingCopy.ClearValue, FieldEditorViewModelTests.NewFieldValue, "The WorkingCopy should have expected values after the commit");
            Assert.AreEqual(this.viewModel.WorkingCopy.Protected, FieldEditorViewModelTests.NewFieldProtected, "The WorkingCopy should have expected values after the commit");
            Assert.AreNotSame(this.viewModel.WorkingCopy, this.parentEntry.Fields[1], "The new field in the entry's collection should not be ref-equal to the WorkingCopy");

            this.viewModel.WorkingCopy.Key = String.Empty;
            Assert.AreNotEqual(this.viewModel.WorkingCopy, this.parentEntry.Fields[1], "Modifying the WorkingCopy should not modify the added field");
        }

        [TestMethod, DetailsFor(isNew: true)]
        public void FieldEditorViewModel_NewFieldExistingKey()
        {
            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.ExistingFieldKey;
            ValidateExistingKey();

            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.NewFieldKey;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should be save-able if it is modified to no longer use a duplicate key");
        }

        [TestMethod, DetailsFor(isNew: false)]
        public void FieldEditorViewModel_ExistingFieldExistingKey()
        {
            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.ExistingFieldKey;
            ValidateExistingKey();

            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.EditedFieldKey;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should be save-able if it is modified to no longer use a duplicate key");
        }

        [TestMethod, DetailsFor(isNew: true)]
        public void FieldEditorViewModel_NewFieldReservedKeys()
        {
            foreach(string reservedKey in ReservedKeys)
            {
                this.viewModel.WorkingCopy.Key = reservedKey;
                ValidateReservedKey();
            }

            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.NewFieldKey;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should be save-able if it is modified to no longer use a reserved key");
        }

        [TestMethod, DetailsFor(isNew: false)]
        public void FieldEditorViewModel_ExistingFieldReservedKeys()
        {
            foreach (string reservedKey in ReservedKeys)
            {
                this.viewModel.WorkingCopy.Key = reservedKey;
                ValidateReservedKey();
            }

            this.viewModel.WorkingCopy.Key = FieldEditorViewModelTests.EditedFieldKey;
            Assert.IsTrue(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should be save-able if it is modified to no longer use a reserved key");
        }

        /// <summary>
        /// Validates ViewModel error state for using a duplicate key
        /// </summary>
        private void ValidateExistingKey()
        {
            Assert.IsFalse(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should not be save-able if it is modified to have a duplicate key");
            Assert.AreEqual(((FieldEditorViewModel)this.viewModel).LocalizedDuplicateKey, this.viewModel.ValidationError, "Duplicating a key should have the right validation error");
        }

        /// <summary>
        /// Validates ViewModel error state for using a reserved key string
        /// </summary>
        private void ValidateReservedKey()
        {
            Assert.IsFalse(this.viewModel.CommitCommand.CanExecute(this.parentEntry), "A field should not be save-able if it is modified to have a reserved key");
            Assert.AreEqual(((FieldEditorViewModel)this.viewModel).LocalizedReservedKey, this.viewModel.ValidationError, "Using a reserved key should have the right validation error");
        }
    }
}
