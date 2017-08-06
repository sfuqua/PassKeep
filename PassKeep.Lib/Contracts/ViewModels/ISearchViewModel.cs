// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
