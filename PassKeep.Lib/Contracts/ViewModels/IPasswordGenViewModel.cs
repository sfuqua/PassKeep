using PassKeep.Lib.Models;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IPasswordGenViewModel
    {
        /// <summary>
        /// The desired length of the password.
        /// </summary>
        int Length
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include lowercase English alphabet characters.
        /// </summary>
        bool UseLowerCase
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include uppercase English alphabet characters.
        /// </summary>
        bool UseUpperCase
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include the digits 0-9.
        /// </summary>
        bool UseDigits
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include the space character.
        /// </summary>
        bool UseSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include miscellaneous symbols.
        /// </summary>
        bool UseSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include miscellaneous brackets.
        /// </summary>
        bool UseBrackets
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include the minus character.
        /// </summary>
        bool UseMinus
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to include the underscore character.
        /// </summary>
        bool UseUnderscore
        {
            get;
            set;
        }

        /// <summary>
        /// A string of extra characters to include.
        /// </summary>
        string AllowList
        {
            get;
            set;
        }

        /// <summary>
        /// A string of characters to exclude.
        /// </summary>
        string ExcludeList
        {
            get;
            set;
        }

        /// <summary>
        /// A command for generating a password to the clipboard.
        /// </summary>
        ICommand ClipboardCopyCommand
        {
            get;
        }

        /// <summary>
        /// Creates a PasswordRecipe based on the current parameters.
        /// </summary>
        /// <returns></returns>
        PasswordRecipe GetCurrentRecipe();
    }
}
