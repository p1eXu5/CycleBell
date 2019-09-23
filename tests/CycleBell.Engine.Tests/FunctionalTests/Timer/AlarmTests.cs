using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CycleBell.Engine.Timer;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.FunctionalTests.Timer
{
    [TestFixture]
    public class AlarmTests
    {
        [ Test ]
        public void PlayDefault_DefaultSoundFolderContainsSounds_CanPlay()
        {
            var alarm = new Alarm( new MediaPlayerFactory() );

            alarm.LoadDefaultSounds();
            Thread.Sleep( 100 );

            alarm.PlayDefault();

            Thread.Sleep( 1000 );
        }
    }
}
