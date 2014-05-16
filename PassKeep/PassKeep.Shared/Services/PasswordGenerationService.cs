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
            if (rngProvider == null)
            {
                throw new ArgumentNullException("rngProvider");
            }

            this.rngProvider = rngProvider;
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
                throw new ArgumentNullException("recipe");
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
                        newPassword += recipe.AvailableCharacters[rngProvider.GetInt(recipe.AvailableCharacters.Count)];
                    }

                    return newPassword;
                }
            );
        }
    }
}
