using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System.Collections.Generic;

namespace PassKeep.Lib.ViewModels
{
    public sealed class SearchViewModel : BindableBase, ISearchViewModel
    {
        private string _query;
        public string Query
        {
            get { return _query; }
            set { SetProperty(ref _query, value); }
        }

        private IList<SearchFilter> _filters;
        public IList<SearchFilter> Filters
        {
            get { return _filters; }
            set { SetProperty(ref _filters, value); }
        }

        private bool _showFilters;
        public bool ShowFilters
        {
            get { return _showFilters; }
            set { SetProperty(ref _showFilters, value); }
        }

        public SearchViewModel(string query)
        {
            Query = query;
        }
    }
}
