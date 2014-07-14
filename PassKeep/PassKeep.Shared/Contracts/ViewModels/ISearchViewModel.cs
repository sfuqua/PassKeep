using PassKeep.Lib.Contracts.Models;
using System.Collections;
using System.Collections.Generic;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface ISearchViewModel : IViewModel
    {
        string Query { get; set; }
        IList<SearchFilter> Filters { get; set; }
        ICollection<IKeePassNode> Results { get; set; }
        bool ShowFilters { get; set; }

        ICollection<IKeePassNode> GetAllNodes();
    }
}
