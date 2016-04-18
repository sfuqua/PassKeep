using System.Threading.Tasks;
using System.Windows.Input;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// <see cref="https://msdn.microsoft.com/en-us/magazine/dn630647.aspx"/>
    /// Thanks Stephen Cleary for the suggestion.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
