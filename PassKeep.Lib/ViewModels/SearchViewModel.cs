// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PassKeep.Lib.ViewModels
{
    public sealed class SearchViewModel : AbstractViewModel, ISearchViewModel
    {
        private IDatabaseViewModel databaseViewModel;

        private string _query;
        public string Query
        {
            get { return _query; }
            set { TrySetProperty(ref _query, value); }
        }

        private IList<SearchFilter> _filters;
        public IList<SearchFilter> Filters
        {
            get { return _filters; }
            set { TrySetProperty(ref _filters, value); }
        }

        private bool _showFilters;
        public bool ShowFilters
        {
            get { return _showFilters; }
            set { TrySetProperty(ref _showFilters, value); }
        }

        private ICollection<IKeePassNode> _results;
        public ICollection<IKeePassNode> Results
        {
            get { return this._results; }
            set { TrySetProperty(ref this._results, value);  }
        }

        public SearchViewModel(string query, IDatabaseViewModel databaseViewModel)
        {
            Query = query;
            this.databaseViewModel = databaseViewModel;
        }

        public ICollection<IKeePassNode> GetAllNodes()
        {
            return this.databaseViewModel.GetAllSearchableNodes(string.Empty).Where(node => node.MatchesQuery(Query)).ToList();
        }
    }
}
