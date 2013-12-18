﻿using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    public class PasswordGenViewModel : BindableBase, IPasswordGenViewModel
    {
        private int _length = 20;
        public int Length
        {
            get { return _length; }
            set { SetProperty(ref _length, value); }
        }

        private bool _useLowerCase = true;
        public bool UseLowerCase
        {
            get { return _useLowerCase; }
            set { SetProperty(ref _useLowerCase, value); }
        }

        private bool _useUpperCase = true;
        public bool UseUpperCase
        {
            get { return _useUpperCase; }
            set { SetProperty(ref _useUpperCase, value); }
        }

        private bool _useDigits = true;
        public bool UseDigits
        {
            get { return _useDigits; }
            set { SetProperty(ref _useDigits, value); }
        }

        private bool _useSpace = false;
        public bool UseSpace
        {
            get { return _useSpace; }
            set { SetProperty(ref _useSpace, value); }
        }

        private bool _useSymbols = false;
        public bool UseSymbols
        {
            get { return _useSymbols; }
            set { SetProperty(ref _useSymbols, value); }
        }

        private bool _useBrackets = false;
        public bool UseBrackets
        {
            get { return _useBrackets; }
            set { SetProperty(ref _useBrackets, value); }
        }

        private bool _useMinus = false;
        public bool UseMinus
        {
            get { return _useMinus; }
            set { SetProperty(ref _useMinus, value); }
        }

        private bool _useUnderscore = false;
        public bool UseUnderscore
        {
            get { return _useUnderscore; }
            set { SetProperty(ref _useUnderscore, value); }
        }

        private string _allowList = string.Empty;
        public string AllowList
        {
            get { return _allowList; }
            set { SetProperty(ref _allowList, value); }
        }

        private string _excludeList = string.Empty;
        public string ExcludeList
        {
            get { return _excludeList; }
            set { SetProperty(ref _excludeList, value); }
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
