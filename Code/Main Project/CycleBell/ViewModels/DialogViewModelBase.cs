using System;
using System.Windows.Input;
using CycleBell.Base;

namespace CycleBell.ViewModels
{
    public abstract class DialogViewModelBase : ObservableObject, IDialogCloseRequested
    {
        public ICommand OkCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(true));});
        public ICommand CancelCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(false));});

        public virtual void OnDialogRequestClose(object sender, DialogRequestCloseEventArgs args)
        {
            DialogCloseRequested?.Invoke(sender, args);
        }

        public virtual event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }
}