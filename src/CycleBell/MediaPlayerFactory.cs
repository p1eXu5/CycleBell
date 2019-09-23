using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CycleBell.Engine.Timer;

namespace CycleBell
{
    public class MediaPlayerFactory : IPlayerFactory
    {
        public IPlayer CreatePlayer()
        {
            return new MediaPlayerWrapper();
        }
    }
}
