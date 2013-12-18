using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    public class PasswordGenerationService : IPasswordGenerationService
    {
        private ICryptoRngProvider rngProvider;

        public PasswordGenerationService(ICryptoRngProvider rngProvider)
        {
            Debug.Assert(rngProvider != null);
            if (rngProvider == null)
            {
                throw new ArgumentNullException("rngProvider");
            }

            this.rngProvider = rngProvider;
        }

        public Task<string> Generate(PasswordRecipe recipe)
        {
            Debug.Assert(recipe != null);
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
