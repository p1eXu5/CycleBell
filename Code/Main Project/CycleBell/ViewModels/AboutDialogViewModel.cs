﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Base;

namespace CycleBell.ViewModels
{
    public class AboutDialogViewModel : DialogViewModelBase
    {
        public override event EventHandler<DialogRequestCloseEventArgs> DialogCloseRequested;
    }
}
