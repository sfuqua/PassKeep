// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using SariphLib.Mvvm;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel responsible for cobbling together passwords based on user-specified parameters.
    /// </summary>
    public class PasswordGenViewModel : BindableBase, IPasswordGenViewModel
    {
        /// <summary>
        /// Initializes the ViewModel with the specified clipboard access.
        /// </summary>
        public PasswordGenViewModel(IPasswordGenerationService passwordService, ISensitiveClipboardService clipboardService)
        {
            ClipboardCopyCommand = new ActionCommand(
                async () =>
                {
                    clipboardService.CopyCredential(
                        await passwordService.Generate(GetCurrentRecipe()),
                        ClipboardOperationType.Password
                    );
                }
            );
        }

        private int _length = 20;
        public int Length
        {
            get { return _length; }
            set { TrySetProperty(ref _length, value); }
        }

        private bool _useLowerCase = true;
        public bool UseLowerCase
        {
            get { return _useLowerCase; }
            set { TrySetProperty(ref _useLowerCase, value); }
        }

        private bool _useUpperCase = true;
        public bool UseUpperCase
        {
            get { return _useUpperCase; }
            set { TrySetProperty(ref _useUpperCase, value); }
        }

        private bool _useDigits = true;
        public bool UseDigits
        {
            get { return _useDigits; }
            set { TrySetProperty(ref _useDigits, value); }
        }

        private bool _useSpace = false;
        public bool UseSpace
        {
            get { return _useSpace; }
            set { TrySetProperty(ref _useSpace, value); }
        }

        private bool _useSymbols = false;
        public bool UseSymbols
        {
            get { return _useSymbols; }
            set { TrySetProperty(ref _useSymbols, value); }
        }

        private bool _useBrackets = false;
        public bool UseBrackets
        {
            get { return _useBrackets; }
            set { TrySetProperty(ref _useBrackets, value); }
        }

        private bool _useMinus = false;
        public bool UseMinus
        {
            get { return _useMinus; }
            set { TrySetProperty(ref _useMinus, value); }
        }

        private bool _useUnderscore = false;
        public bool UseUnderscore
        {
            get { return _useUnderscore; }
            set { TrySetProperty(ref _useUnderscore, value); }
        }

        private string _allowList = string.Empty;
        public string AllowList
        {
            get { return _allowList; }
            set { TrySetProperty(ref _allowList, value); }
        }

        private string _excludeList = string.Empty;
        public string ExcludeList
        {
            get { return _excludeList; }
            set { TrySetProperty(ref _excludeList, value); }
        }

        /// <summary>
        /// A command for generating a password to the clipboard.
        /// </summary>
        public ICommand ClipboardCopyCommand
        {
            get;
            private set;
        }

        public PasswordRecipe GetCurrentRecipe()
        {
            PasswordRecipe recipe = new PasswordRecipe(Length);

            if (UseLowerCase)
            {
                recipe.Include("abcdefghijklmnopqrstuvwxyz");
            }

            if (UseUpperCase)
            {
                recipe.Include("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }

            if (UseDigits)
            {
                recipe.Include("0123456789");
            }

            if (UseSpace)
            {
                recipe.Include(' ');
            }

            if (UseSymbols)
            {
                recipe.Include("!@#$%^&*/\\'\"`");
            }

            if (UseBrackets)
            {
                recipe.Include("<>{}[]()");
            }

            if (UseMinus)
            {
                recipe.Include('-');
            }

            if (UseUnderscore)
            {
                recipe.Include('_');
            }

            // Handle the final two lists of exceptions
            recipe.Include(AllowList);
            recipe.Exclude(ExcludeList);

            return recipe;
        }
    }
}
