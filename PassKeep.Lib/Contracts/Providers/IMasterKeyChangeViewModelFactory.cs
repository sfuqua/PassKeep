using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface IMasterKeyChangeViewModelFactory
    {
        IMasterKeyViewModel Assemble(KdbxDocument document, ITestableFile databaseFile);
    }
}
