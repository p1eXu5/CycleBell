using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

    public interface IDialogCloseRequested
    {
        event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }

    public class DialogRequestCloseEventArgs : EventArgs
    {
        public DialogRequestCloseEventArgs(bool? dialogResult, object tag = null)
        {
            DialogResult = dialogResult;
            Tag = tag;
        }

        public bool? DialogResult { get; }
        public object Tag { get; }
    }

    public interface IDialogRegistrator
    {
        void Register<TViewModel, TView>() where      TView : IDialog
            where TViewModel : IDialogCloseRequested;

        bool? ShowDialog<TViewModel>(TViewModel viewModel, Predicate<object> predicate = null) where TViewModel: IDialogCloseRequested;
    }

    public class DialogRegistrator : IDialogRegistrator
    {
        private Dictionary<Type, Type> _map;
        private Window _owner;

        public DialogRegistrator(Window window)
        {
            _map = new Dictionary<Type, Type>();
            _owner = window;
        }


        public void Register<TViewModel, TView>() where      TView : IDialog
                                                  where TViewModel : IDialogCloseRequested
        {
            Type viewModelType = typeof(TViewModel);
            if (_map.ContainsKey(viewModelType)) {
                throw new ArgumentException($"{viewModelType} already exist in dictionary");
            }

            _map[viewModelType] = typeof(TView);
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel, Predicate<object> predicate = null) where TViewModel: IDialogCloseRequested
        {
            Type viewType;

            if (_map.TryGetValue(typeof(TViewModel), out viewType)) {

                IDialog wnd = (IDialog)Activator.CreateInstance(viewType);
                wnd.Owner = _owner;
                wnd.DataContext = viewModel;

                EventHandler<DialogRequestCloseEventArgs> handler = null;

                handler += (sender, args) =>
                {
                    if (predicate != null && !predicate(args.Tag))
                        return;

                    viewModel.DialogCloseRequested -= handler;

                    if (args.DialogResult.HasValue) {

                        // Присвоение значения свойству DialogResult диалогового окна закрывает его
                        wnd.DialogResult = args.DialogResult;
                    }
                    else {

                        wnd.Close();
                    }
                };

                viewModel.DialogCloseRequested += handler;

                return wnd.ShowDialog();
            }

            return null;
        }
    }
}
