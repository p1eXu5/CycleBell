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

namespace CycleBell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Window wnd = new MainWindow();

            Registrator registrator = new Registrator(wnd);
            registrator.Register<AboutDialogViewModel,AboutWindow>();

            wnd.DataContext = new CycleBellViewModel(registrator);
            wnd.ShowDialog();
        }
    }
}
