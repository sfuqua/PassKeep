using System.Threading.Tasks;
using PassKeep.Lib.Models;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// An interface for services for generating passwords in a cryptographically secure fashion.
    /// </summary>
    public interface IPasswordGenerationService
    {
        /// <summary>
        /// Asynchronously generates a password from the given recipe.
        /// </summary>
        /// <param name="recipe">A description of the password to create.</param>
        /// <returns>An async operation that will contain the string.</returns>
        Task<string> Generate(PasswordRecipe recipe);
    }
}
