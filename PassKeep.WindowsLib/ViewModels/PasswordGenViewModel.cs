using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using SariphLib.MVVM;

namespace PassKeep.Lib.ViewModels
{
    public class PasswordGenViewModel : BindableBase, IPasswordGenViewModel
    {
        private int length = 20;
        public int Length
        {
            get { return length; }
            set { SetProperty(ref length, value); }
        }

        private bool useLowerCase = true;
        public bool UseLowerCase
        {
            get { return useLowerCase; }
            set { SetProperty(ref useLowerCase, value); }
        }

        private bool useUpperCase = true;
        public bool UseUpperCase
        {
            get { return useUpperCase; }
            set { SetProperty(ref useUpperCase, value); }
        }

        private bool useDigits = true;
        public bool UseDigits
        {
            get { return useDigits; }
            set { SetProperty(ref useDigits, value); }
        }

        private bool useSpace = false;
        public bool UseSpace
        {
            get { return useSpace; }
            set { SetProperty(ref useSpace, value); }
        }

        private bool useSymbols = false;
        public bool UseSymbols
        {
            get { return useSymbols; }
            set { SetProperty(ref useSymbols, value); }
        }

        private bool useBrackets = false;
        public bool UseBrackets
        {
            get { return useBrackets; }
            set { SetProperty(ref useBrackets, value); }
        }

        private bool useMinus = false;
        public bool UseMinus
        {
            get { return useMinus; }
            set { SetProperty(ref useMinus, value); }
        }

        private bool useUnderscore = false;
        public bool UseUnderscore
        {
            get { return useUnderscore; }
            set { SetProperty(ref useUnderscore, value); }
        }

        private string allowList = string.Empty;
        public string AllowList
        {
            get { return allowList; }
            set { SetProperty(ref allowList, value); }
        }

        private string excludeList = string.Empty;
        public string ExcludeList
        {
            get { return excludeList; }
            set { SetProperty(ref excludeList, value); }
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
