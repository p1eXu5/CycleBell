using System;

namespace CycleBell.Base {
    public interface IDialogCloseRequested
    {
        event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }
}