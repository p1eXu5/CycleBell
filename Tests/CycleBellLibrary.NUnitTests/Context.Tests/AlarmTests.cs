using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBell;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Context.Tests
{
    [ TestFixture ]
    public class AlarmTests
    {
        [ Test ]
        public void Ctor_ByDefault_ClonePlayer ()
        {
            var alarm = new Alarm( new MediaPlayer_() );

            Assert.That( !ReferenceEquals( alarm.DefaultPlayer, alarm.Player ) );
            Assert.That( alarm.Player, Is.Not.Null );
            Assert.That( alarm.DefaultPlayer, Is.Not.Null );
        }
    }
}
