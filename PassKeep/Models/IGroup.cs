using System;

namespace PassKeep.Models
{
    public interface IGroup
    {
        KdbxString Title { get; set; }
        KeePassUuid Uuid { get; set; }
        KdbxString Notes { get; set; }

        bool MatchesQuery(string query);
    }
}
