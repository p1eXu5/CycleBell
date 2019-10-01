using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.Views;
using CycleBell.Views.Windows;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;
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
            
            SetupUiCulture();

            Window wnd = new MainWindow();

            var dialogRegistrator = RegisterDialogs( wnd );
            var manager = new CycleBellManager ("presets.xml", new PresetCollection(), TimerManager.Instance);
            var container = RegisterTypes( dialogRegistrator, manager );

            try {
                wnd.DataContext = container.Resolve< MainViewModel >();
            }
            catch (Exception ex) {
                Trace.WriteLine (ex.Message);
            }

            wnd.Closed += delegate( object sender, EventArgs args ) { manager.SavePresets(); };
            wnd.Show();
        }

        private static UnityContainer RegisterTypes( DialogRegistrator dialogRegistrator, CycleBellManager manager )
        {
            var container = new UnityContainer();
            container.RegisterInstance< IDialogRegistrator >( dialogRegistrator );

            var alarm = new Alarm( new MediaPlayerFactory() );
            alarm.LoadDefaultSoundCollection();
            alarm.SetDefaultSound();

            container.RegisterInstance< IAlarm >( alarm );

            container.RegisterInstance< ICycleBellManager >( manager );

            container.RegisterType< MainViewModel >();
            return container;
        }

        private static DialogRegistrator RegisterDialogs( Window wnd )
        {
            DialogRegistrator dialogRegistrator = new DialogRegistrator( wnd );
            dialogRegistrator.Register< AboutDialogViewModel, AboutWindow >();
            dialogRegistrator.Register< SavePresetDialogViewModel, SavePresetDialogWindow >();
            dialogRegistrator.Register< RenamePresetDialogViewModel, RenamePresetDialogWindow >();
            return dialogRegistrator;
        }

        private static void SetupUiCulture()
        {
            try {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo( "en-us" );
            }
            catch ( CultureNotFoundException ) { }
        }
    }
}
