using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary.Context
{
    public interface IPlayer
    {
        void Open ( Uri source );
        void Play ();
        void Stop ();
        bool HasAudio { get; }
        event EventHandler MediaEnded;
        IPlayer ClonePlayer ();
        Uri Source { get; }
    }
}
