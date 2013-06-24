using PassKeep.Contracts.Models;
using Windows;

namespace PassKeep.Shared.Contracts
{
    public interface IPasswordGenerationService
    {
        /// <summary>
        /// Asynchronously generates a password from the given recipe.
        /// </summary>
        /// <param name="recipe">A description of the password to create.</param>
        /// <returns>An async operation that will contain the string.</returns>
        IAsyncOperation<string> Generate(IPasswordRecipe recipe);
    }
}
