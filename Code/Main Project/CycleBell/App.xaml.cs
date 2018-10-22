﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
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
            // TODO tasks:
            //      5.  Import presets
            //      7.  Help
            //      8.  License
            //      9.  Deploy
            try {

                Thread.CurrentThread.CurrentUICulture = new CultureInfo ("en-us");

                base.OnStartup (e);

                TimePoint.DefaultTimePointNameFunc = point => $"Time point {point.Id}";

                var container = new UnityContainer();

                Window wnd = new MainWindow();

                // IDialogRegistrator:
                DialogRegistrator dialogRegistrator = new DialogRegistrator (wnd);
                dialogRegistrator.Register<AboutDialogViewModel, AboutWindow>();
                dialogRegistrator.Register<SavePresetDialogViewModel, SavePresetDialogWindow>();
                dialogRegistrator.Register<RenamePresetDialogViewModel, RenamePresetDialogWindow>();

                container.RegisterInstance<IDialogRegistrator> (dialogRegistrator);

                // ITimerManager:
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
