// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
