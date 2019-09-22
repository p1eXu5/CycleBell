using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Engine.Timer
{
    public interface IPlayer
    {
        Uri Source { get; }
        bool HasAudio { get; }

        //event EventHandler MediaEnded;

        void Open ( Uri source );
        void Play ();
        void Stop ();
        IPlayer ClonePlayer ();
    }
}
