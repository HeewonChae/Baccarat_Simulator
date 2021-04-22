using BaccaratSimulator.Logic;
using LogicCore.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BaccaratSimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQzNTA2QDMxMzgyZTMxMmUzMEk5TlYrL2swcThIUCtsZllqRXFzZWx6YituY3VOTW15RS8wVHZEbjVOOWc9");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Singleton.Register<BaccaratCore>();
            Singleton.Register<BaccaratPlay>();
        }
    }
}