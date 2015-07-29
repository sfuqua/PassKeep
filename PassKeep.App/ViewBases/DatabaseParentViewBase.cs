using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.UI.Xaml.Controls;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseParentViewBase : HostingPassKeepPage<IDatabaseParentViewModel>, IHostingPage
    {
    }
}
