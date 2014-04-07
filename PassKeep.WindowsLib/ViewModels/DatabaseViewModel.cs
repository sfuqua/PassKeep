using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    public class DatabaseViewModel : BindableBase, IDatabaseViewModel
    {
        private IDatabasePersistenceService persistenceService;

        /// <summary>
        /// Initializes the ViewModel with the provided parameters.
        /// </summary>
        /// <param name="document">The document this ViewModel will represent.</param>
        /// <param name="navigationViewModel">A ViewModel representing navigation state.</param>
        /// <param name="persistenceService">The service used to save the database.</param>
        public DatabaseViewModel(
            KdbxDocument document,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService
        )
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (navigationViewModel == null)
            {
                throw new ArgumentNullException("breadcrumbViewModel");
            }
             
            if (persistenceService == null)
            {
                throw new ArgumentNullException("persistenceService");
            }

            this.Document = document;
            this.NavigationViewModel = navigationViewModel;
            this.persistenceService = persistenceService;
        }

        /// <summary>
        /// The navigation ViewModel for the database.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the database.
        /// </remarks>
        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        public KdbxDocument Document
        {
            get;
            private set;
        }       

        /// <summary>
        /// Attempts to save the current state of the database to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        public Task<bool> TrySave()
        {
            return this.persistenceService.Save(this.Document);
        }

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        public ICollection<IKeePassNode> GetAllSearchableNodes()
        {
            IList<IKeePassNode> items = new List<IKeePassNode>();
            AddToSearchCollection(Document.Root.DatabaseGroup, items);
            return items;
        }

        /// <summary>
        /// Recursive function for updating collecting a full listing of searchable database nodes.
        /// </summary>
        /// <param name="root">The root of the recursive probe.</param>
        /// <param name="soFar">The list of searchable nodes gathered so far.</param>
        private void AddToSearchCollection(IKeePassGroup root, IList<IKeePassNode> soFar)
        {
            if (soFar == null)
            {
                throw new ArgumentNullException("soFar");
            }

            if (root == null)
            {
                return;
            }

            // Keep in mind that all groups are searched. The search flag only applies to entries within a group.
            soFar.Add(root);

            // Add recurse on child groups
            var groups = root.Groups;
            foreach (var group in groups)
            {
                AddToSearchCollection(group, soFar);
            }

            // Do not add entries if the group is not searchable.
            if (!root.IsSearchingPermitted())
            {
                return;
            }

            var entries = root.Entries;
            foreach (var entry in entries)
            {
                soFar.Add(entry);
            }
        }
    }
}
