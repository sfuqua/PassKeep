// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
