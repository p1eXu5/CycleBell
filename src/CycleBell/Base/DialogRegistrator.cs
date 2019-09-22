using System;
using System.Collections.Generic;
using System.Windows;

namespace CycleBell.Base
{
    public class DialogRegistrator : IDialogRegistrator
    {
        private readonly Dictionary<Type, Type> _map;
        private readonly Window _owner;

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

                // create tmp delegate
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
