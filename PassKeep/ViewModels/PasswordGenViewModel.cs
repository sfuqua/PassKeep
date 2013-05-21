using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;

namespace PassKeep.ViewModels
{
    public class PasswordGenViewModel : ViewModelBase
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

        public PasswordGenViewModel(ConfigurationViewModel appSettings)
            : base(appSettings) { }

        public async Task<string> Generate()
        {
            // Short-circuit the async bit if there's no work to do.
            if (Length <= 0)
            {
                return string.Empty;
            }

            StringBuilder password = new StringBuilder(Length);
            // Compute the password asynchronously - depending on the RNG it might take a while.
            await Task.Run(() =>
                {
                    StringBuilder availableCharacterBuilder = new StringBuilder();
                    if (UseLowerCase)
                    {
                        availableCharacterBuilder.Append("abcdefghijklmnopqrstuvwxyz");
                    }
                    if (UseUpperCase)
                    {
                        availableCharacterBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    }
                    if (UseDigits)
                    {
                        availableCharacterBuilder.Append("0123456789");
                    }
                    if (UseSpace)
                    {
                        availableCharacterBuilder.Append(' ');
                    }
                    if (UseSymbols)
                    {
                        availableCharacterBuilder.Append("!@#$%^&*/\\'\"`");
                    }
                    if (UseBrackets)
                    {
                        availableCharacterBuilder.Append("<>{}[]()");
                    }
                    if (useMinus)
                    {
                        availableCharacterBuilder.Append('-');
                    }
                    if (useUnderscore)
                    {
                        availableCharacterBuilder.Append('_');
                    }

                    // Append all additional allow characters (guaranteeing uniqueness)
                    string availableCharacters = availableCharacterBuilder.ToString();
                    for (int i = 0; i < AllowList.Length; i++)
                    {
                        if (availableCharacters.IndexOf(AllowList[i]) < 0)
                        {
                            availableCharacters += AllowList[i];
                        }
                    }

                    // Remove all dis-allowed characters
                    for (int i = 0; i < ExcludeList.Length; i++)
                    {
                        int index = availableCharacters.IndexOf(ExcludeList[i]);
                        if (index >= 0)
                        {
                            availableCharacters = availableCharacters.Remove(index, 1);
                        }
                    }

                    // Short-circuit if we've removed all allowed characters
                    if (availableCharacters.Length == 0)
                    {
                        return;
                    }

                    Random gen = new Random();
                    for (int i = 0; i < Length; i++)
                    {
                        int index = (int)(CryptographicBuffer.GenerateRandomNumber() % availableCharacters.Length);
                        char newChar = availableCharacters[index];
                        password.Append(newChar);
                    }
                }
            );

            // Return the calculated password.
            return password.ToString();
        }
    }
}
