using System;
using System.Windows;

namespace CycleBell.Base
{
    public interface IDialog
    {
        bool? ShowDialog();
        bool? DialogResult { get; set; }
        Window Owner { get; set; }
        Object DataContext { get; set; }
        void Close();
    }
}