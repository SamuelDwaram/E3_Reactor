using Anathem.Shell.Views;
using Prism.Ioc;
using Prism.Modularity;
using SingleInstanceUtilities;
using System;
using System.Windows;

namespace Anathem.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly SingleInstance Singleton = new SingleInstance(typeof(App).FullName);

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Singleton.RunFirstInstance(() => {
                App app = new App();
                app.InitializeComponent();
                app.Run();
            });
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }
    }
}
