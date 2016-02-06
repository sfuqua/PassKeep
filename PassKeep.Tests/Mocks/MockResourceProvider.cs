using PassKeep.Lib.Contracts.Providers;
namespace PassKeep.Tests.Mocks
{
    public class MockResourceProvider : IResourceProvider
    {
        public string GetString(string key)
        {
            return "DUMMY";
        }
    }
}
