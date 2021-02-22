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
            Menu.MainMenu mainMenu = new Menu.MainMenu();
            mainMenu.DataContext = new Menu.MainMenuViewModel(DialogCoordinator.Instance);
            mainMenu.Show();
        }
    }
}
