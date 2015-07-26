using PassKeep.Lib.Contracts.Providers;
using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Providers
{
    public class DispatcherTimerFactory : ITimerFactory
    {
        public ITimer Assemble()
        {
            return new ConcreteDispatcherTimer();
        }

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="interval">Interval to use for the new timer.</param>
        /// <returns></returns>
        public ITimer Assemble(TimeSpan interval)
        {
            return new ConcreteDispatcherTimer()
            {
                Interval = interval
            };
        }
    }
}
