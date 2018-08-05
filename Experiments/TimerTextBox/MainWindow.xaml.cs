using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimerTextBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnMouseDoubleClick (MouseButtonEventArgs e)
        {
            m_labels.Visibility = Visibility.Hidden;
            m_timer.Visibility = Visibility.Visible;
            m_timer.Focus();
            m_timer.SelectAll();
            base.OnMouseDoubleClick (e);
        }


        private void MainWindow_OnLostFocus (object sender, RoutedEventArgs e)
        {
            m_timer.Visibility = Visibility.Hidden;
            m_labels.Visibility = Visibility.Visible;
        }
    }
}
