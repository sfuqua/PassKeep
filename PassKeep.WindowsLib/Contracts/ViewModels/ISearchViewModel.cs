using System.Collections.Generic;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface ISearchViewModel : IViewModel
    {
        string Query { get; set; }
        IList<SearchFilter> Filters { get; set; }
        bool ShowFilters { get; set; }
    }
}
