using System.Windows;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.Views;
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
            //      2.  Correct IsEnable triggers in data templates
            //      4.  PresetsComboBox (layers or template?), save, save as
            //      5.  Export/Import presets
            //      6.  LoopNumber ComboBox
            //      7.  Help
            //      8.  License
            //      9.  Deploy
            //      10. DrugAndDrop?

            base.OnStartup (e);

            TimePoint.DefaultTimePointNameFunc = point => $"Time point {point.Id}";

            var container = new UnityContainer();

            Window wnd = new MainWindow();

            // IDialogRegistrator:
            DialogRegistrator dialogRegistrator = new DialogRegistrator(wnd);
            dialogRegistrator.Register<AboutDialogViewModel,AboutWindow>();

            container.RegisterInstance<IDialogRegistrator>(dialogRegistrator);

            // ITimerManager:
            var manager =  new CycleBellManager("test.xml", new PresetCollectionManager(), TimerManager.Instance);;

            container.RegisterInstance<ICycleBellManager>(manager);

            container.RegisterType<MainViewModel>();

            wnd.DataContext = container.Resolve<MainViewModel>();

            wnd.ShowDialog();
        }
    }
}
