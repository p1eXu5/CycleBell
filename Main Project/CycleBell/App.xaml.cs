using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CycleBell.Base;
using CycleBell.ModelViews;
using CycleBell.Views;
using CycleBellLibrary;
using Unity;

namespace CycleBell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup (e);

            var container = new UnityContainer();

            Window wnd = new MainWindow();

            DialogRegistrator dialogRegistrator = new DialogRegistrator(wnd);
            dialogRegistrator.Register<AboutDialogViewModel,AboutWindow>();
            container.RegisterInstance<IDialogRegistrator>(dialogRegistrator);

            var manager = TimerManager.Instance (new PresetsManager());
            container.RegisterInstance<ITimerManager>(manager);

            container.RegisterType<MainViewModel>();

            wnd.DataContext = container.Resolve<MainViewModel>();

            wnd.ShowDialog();
        }
    }
}
