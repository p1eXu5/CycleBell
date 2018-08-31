﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public class EndTimePointViewModel : TimePointViewModelBase
    {
        public EndTimePointViewModel(byte loopNumber) : base(TimePoint.MaxId, loopNumber)
        {
        }
    }
}