using System;

namespace CycleBell.Base
{
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
}