using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Base;

namespace CycleBell.ModelViews
{
    public class AboutDialogViewModel : IDialogCloseRequested
    {
        public ActionCommand OkCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(true));});
        public ActionCommand CancelCommand => new ActionCommand(p => { OnDialogRequestClose(this, new DialogRequestCloseEventArgs(false));});

        public void OnDialogRequestClose(object sender, DialogRequestCloseEventArgs args)
        {
            DialogCloseRequested?.Invoke(sender, args);
        }

        public event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }
}
