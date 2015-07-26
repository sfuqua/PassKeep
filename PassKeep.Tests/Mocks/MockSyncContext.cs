using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;

namespace PassKeep.Tests.Mocks
{
    public class MockSyncContext : ISyncContext
    {
        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public Task Post(Action activity)
        {
            activity();
            return Task.FromResult<int>(0);
        }
    }
}
