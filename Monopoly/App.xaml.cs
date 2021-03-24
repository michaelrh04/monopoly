using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Monopoly.Game;

namespace Monopoly
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// This code is run upon the first opening of the application
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Constructor for app
        /// </summary>
        public App()
        {
            // Disable hardware acceleration
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            // Let's go!
            Menu.MainMenu View = new Menu.MainMenu();
            // Establish an action which allows the VM to close the V and then assign the VM.
            var ViewModel = new Menu.MainMenuViewModel(DialogCoordinator.Instance);
            ViewModel.Close = new Action(() => View.Close());
            View.DataContext = ViewModel;
            View.Show();
        }
    }
}
