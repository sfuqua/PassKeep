using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface ITimerFactory
    {
        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <returns></returns>
        ITimer Assemble();

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="interval">Interval to use for the new timer.</param>
        /// <returns></returns>
        ITimer Assemble(TimeSpan interval);
    }
}
