using System;
using CycleBell.Base;

namespace CycleBell.ViewModels
{
    public abstract class DialogViewModelBase : ObservableObject, IDialogCloseRequested
    {
        public ActionCommand OkCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(true));});
        public ActionCommand CancelCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(false));});

        public void OnDialogRequestClose(object sender, DialogRequestCloseEventArgs args)
        {
            DialogCloseRequested?.Invoke(sender, args);
        }

        public virtual event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }
}