using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace WPFModernVerticalMenu
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void CheckForUpdates()
        {
            using (var mgr = new UpdateManager("https://mon-serveur.com/releases"))
            {
                await mgr.UpdateApp();
            }
        }
    }
}
