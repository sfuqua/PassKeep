using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.ViewBases;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// The primary View over the contents of a KeePass database.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {
        public DatabaseView()
            : base()
        {
            this.InitializeComponent();

            this.ViewModel.StartedSave;
            this.ViewModel.StoppedSave;
        }

        #region Auto-event handles

        /// <summary>
        /// Auto-event handler for saving a database.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancellableEventArgs for the operation.</param>
        public void StartedSaveHandler(object sender, CancellableEventArgs e)
        {

        }

        /// <summary>
        /// Auto-event handler for when a save operation has stopped.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs for the notification.</param>
        public void StoppedSaveHandler(object sender, EventArgs e)
        {
            this.RaiseDoneLoading();
        }

        #endregion
    }
}
