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
using System.Windows.Shapes;
using CycleBell.Base;

namespace CycleBell.Views.Windows
{
    /// <summary>
    /// Interaction logic for RenamePresetDialogWindow.xaml
    /// </summary>
    public partial class RenamePresetDialogWindow : Window, IDialog
    {
        public RenamePresetDialogWindow()
        {
            InitializeComponent();
        }
    }
}
