using System.Collections.Generic;

namespace PassKeep.Contracts.Models
{
    public interface IPasswordRecipe
    {
        int Length { get; }
        IReadOnlyList<char> AvailableCharacters { get; }
        IReadOnlyList<char> ForbiddenCharacters { get; }
    }
}
