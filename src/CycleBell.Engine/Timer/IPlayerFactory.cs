using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Engine.Timer
{
    public interface IPlayerFactory
    {
        IPlayer CreatePlayer();
    }
}
