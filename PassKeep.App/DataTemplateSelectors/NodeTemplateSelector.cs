// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.DataTemplateSelectors
{
    /// <summary>
    /// A DataTemplateSelector to use for rendering combined collections of IKeePassNodes.
    /// </summary>
    public abstract class NodeTemplateSelector : DataTemplateSelector
    {
        private const string GroupPrefix = "GroupTemplate";
        private const string EntryPrefix = "EntryTemplate";

        protected abstract string Suffix { get; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null)
            {
                return null;
            }

            DataTemplate groupTemplate = (DataTemplate)Application.Current.Resources[
                String.Format("{0}{1}", NodeTemplateSelector.GroupPrefix, Suffix)
            ];
            DataTemplate entryTemplate = (DataTemplate)Application.Current.Resources[
                String.Format("{0}{1}", NodeTemplateSelector.EntryPrefix, Suffix)
            ];

            IDatabaseNodeViewModel node = item as IDatabaseNodeViewModel;
            if (node == null)
            {
                return null;
            }

            IDatabaseEntryViewModel entry = node as IDatabaseEntryViewModel;
            if (entry == null)
            {
                // Group only
                return groupTemplate;
            }
            else
            {
                // Entry
                return entryTemplate;
            }
        }
    }

    /// <summary>
    /// The NodeTemplateSelector to use for full-width views.
    /// </summary>
    public class FullNodeTemplateSelector : NodeTemplateSelector
    {
        protected override string Suffix
        {
            get { return string.Empty; }
        }
    }

    /// <summary>
    /// The NodeTemplateSelector to use for narrow views.
    /// </summary>
    public class NarrowNodeTemplateSelector : NodeTemplateSelector
    {
        protected override string Suffix
        {
            get { return "_Snapped"; }
        }
    }
}
