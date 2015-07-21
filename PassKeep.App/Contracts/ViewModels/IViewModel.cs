using System.ComponentModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Sets up event listeners for dependent services, other repeatable initialization steps.
        /// </summary>
        void Activate();

        /// <summary>
        /// Releases any resources acquired by <see cref="Activate"/>, until the next acquisition.
        /// </summary>
        void Suspend();
    }
}
