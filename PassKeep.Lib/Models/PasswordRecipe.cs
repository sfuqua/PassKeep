using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PassKeep.Lib.Models
{
    /// <summary>
    /// Describes requirements from which to generate a password.
    /// </summary>
    public class PasswordRecipe
    {
        private int _length;
        /// <summary>
        /// How long the password should be when generated. Never negative.
        /// </summary>
        public int Length
        {
            get { return _length; }
            set
            {
                Dbg.Assert(value >= 0);
                _length = Math.Max(0, value);
            }
        }

        private HashSet<char> _availableCharacters;

        /// <summary>
        /// The characters from which to generate the password.
        /// </summary>
        public IReadOnlyList<char> AvailableCharacters
        {
            get
            {
                return _availableCharacters.ToList();
            }
        }

        /// <summary>
        /// Instantiates a PasswordRecipe with no available characters and a length of 0.
        /// </summary>
        public PasswordRecipe()
            : this(0) { }

        /// <summary>
        /// Instantiates a PasswordRecipe with no available characters and the specified length.
        /// </summary>
        /// <param name="length">How long the generated password should be.</param>
        public PasswordRecipe(int length)
        {
            Length = length;
            _availableCharacters = new HashSet<char>();
        }

        /// <summary>
        /// Includes all of the specified characters in the recipe.
        /// </summary>
        /// <param name="characters">A string of characters to individually include.</param>
        public void Include(string characters)
        {
            Dbg.Assert(characters != null);
            if (characters == null)
            {
                throw new ArgumentNullException("characters");
            }

            foreach (char c in characters)
            {
                Include(c);
            }
        }

        /// <summary>
        /// Includes the specified character in the recipe.
        /// </summary>
        /// <param name="c">A character to include.</param>
        public void Include(char c)
        {
            _availableCharacters.Add(c);
        }

        /// <summary>
        /// Excludes all of the specified characters from the recipe.
        /// </summary>
        /// <param name="characters">A string of characters to individually exclude.</param>
        public void Exclude(string characters)
        {
            Dbg.Assert(characters != null);
            if (characters == null)
            {
                throw new ArgumentNullException("characters");
            }

            foreach (char c in characters)
            {
                Exclude(c);
            }
        }

        /// <summary>
        /// Excludes the specified character from the recipe.
        /// </summary>
        /// <param name="c">A character to exclude.</param>
        public void Exclude(char c)
        {
            _availableCharacters.Remove(c);
        }
    }
}
