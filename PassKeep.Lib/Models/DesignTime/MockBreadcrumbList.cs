// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System.Collections.Generic;

namespace PassKeep.Models.DesignTime
{
    public class MockBreadcrumbList
    {
        public MockBreadcrumbList()
        {
            Breadcrumbs = new List<Breadcrumb>
            {
                new Breadcrumb(
                    new MockGroup
                    {
                        Title = new KdbxString("Title", "RootGroup", null)
                    },
                    true
                ),
                new Breadcrumb(
                    new MockGroup
                    {
                        Title = new KdbxString("Title", "SubGroup", null)
                    },
                    false
                ),
                new Breadcrumb(
                    new MockGroup
                    {
                        Title = new KdbxString("Title", "Inner Child Group", null)
                    },
                    false
                ),
                new Breadcrumb(
                    new MockGroup
                    {
                        Title = new KdbxString("Title", "Active Group", null)
                    },
                    false
                ),
            };
        }

        public List<Breadcrumb> Breadcrumbs
        {
            get;
            set;
        }
    }
}
