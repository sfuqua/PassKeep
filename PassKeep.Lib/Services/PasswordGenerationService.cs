// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Models;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A service for generating passwords in a cryptographically secure fashion.
    /// </summary>
    public class PasswordGenerationService : IPasswordGenerationService
    {
        // The cryptographically secure random number generator used for
        // generation.
        private ICryptoRngProvider rngProvider;

        /// <summary>
        /// Initializes the class with the specified random number generator.
        /// </summary>
        /// <param name="rngProvider">The secure random number generator to use.</param>
        public PasswordGenerationService(ICryptoRngProvider rngProvider)
        {
            this.rngProvider = rngProvider ?? throw new ArgumentNullException(nameof(rngProvider));
        }

        /// <summary>
        /// Asynchronously generates a password using the specified PasswordRecipe.
        /// </summary>
        /// <param name="recipe">A description of what constitutes a valid password to generate.</param>
        /// <returns>A Task responsible for generating the desired password.</returns>
        public Task<string> Generate(PasswordRecipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            // Run the generation asynchronously because it could be expensive.
            return Task.Run(
                () =>
                {
                    string newPassword = string.Empty;

                    // Short circuit if there are no permissible characters left.
                    if (recipe.AvailableCharacters.Count == 0)
                    {
                        return newPassword;
                    }

                    // Generate the password from the allowed chars (guaranteed to be good, now)
                    for (int i = 0; i < recipe.Length; i++)
                    {
                        newPassword += recipe.AvailableCharacters[this.rngProvider.GetInt(recipe.AvailableCharacters.Count)];
                    }

                    return newPassword;
                }
            );
        }
    }
}
