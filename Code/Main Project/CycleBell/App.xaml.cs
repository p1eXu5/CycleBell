using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.Views;
using CycleBell.Views.Windows;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
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
            // TODO CycleBell tasks for:
            //      7.  Help
            //      10. Drug&Drop time points
            //      12. Export Selected preset
            try {

                Thread.CurrentThread.CurrentUICulture = new CultureInfo ("en-us");

                base.OnStartup (e);


                Window wnd = new MainWindow();

                // IDialogRegistrator:
                DialogRegistrator dialogRegistrator = new DialogRegistrator (wnd);
                dialogRegistrator.Register<AboutDialogViewModel, AboutWindow>();
                dialogRegistrator.Register<SavePresetDialogViewModel, SavePresetDialogWindow>();
                dialogRegistrator.Register<RenamePresetDialogViewModel, RenamePresetDialogWindow>();

                var container = new UnityContainer();
                container.RegisterInstance<IDialogRegistrator> (dialogRegistrator);

                var alarm = new Alarm( new MediaPlayer_() );
                container.RegisterInstance< IAlarm >( alarm );

                var manager = new CycleBellManager ("test.xml", new PresetCollectionManager(), TimerManager.Instance);
                container.RegisterInstance<ICycleBellManager> (manager);

                container.RegisterType<MainViewModel>();

                wnd.DataContext = container.Resolve<MainViewModel>();
                wnd.ShowDialog();

                manager.SavePresets();
            }
            catch (Exception ex) {

                Debug.WriteLine (ex.Message);
            }
        }
    }
}
