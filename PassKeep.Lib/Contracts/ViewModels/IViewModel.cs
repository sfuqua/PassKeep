using System.ComponentModel;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Sets up event listeners for dependent services, other repeatable initialization steps.
        /// </summary>
        Task ActivateAsync();

        /// <summary>
        /// Releases any resources acquired by <see cref="Activate"/>, until the next acquisition.
        /// </summary>
        Task SuspendAsync();

        /// <summary>
        /// Saves state when app is being suspended.
        /// </summary>
        void HandleAppSuspend();

        /// <summary>
        /// Restores state when app is resumed.
        /// </summary>
        void HandleAppResume();
    }
}
