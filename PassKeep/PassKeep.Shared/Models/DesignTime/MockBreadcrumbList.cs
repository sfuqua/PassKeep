using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System.Collections.Generic;

namespace PassKeep.Models.DesignTime
{
    public class MockBreadcrumbList
    {
        public MockBreadcrumbList()
        {
            this.Breadcrumbs = new List<Breadcrumb>
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
