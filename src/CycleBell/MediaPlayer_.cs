﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CycleBell.Engine.Timer;

namespace CycleBell
{
    public class MediaPlayer_ : MediaPlayer, IPlayer
    {
        public IPlayer ClonePlayer ()
        {
            return new MediaPlayer_();
        }
    }
}