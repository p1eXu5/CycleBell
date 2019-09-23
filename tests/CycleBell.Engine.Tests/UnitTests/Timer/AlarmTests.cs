using System;
using CycleBell.Engine.Timer;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.UnitTests.Timer
{
    [ TestFixture ]
    public class AlarmTests
    {
        [Test]
        public void Ctor_PlayerFactoryIsNull_Throws()
        {
            var ex = Assert.Catch< ArgumentNullException >( () => new Alarm( null ) );
        }


        [ Test ]
        public void LoadDefaultSounds_FolderContainsFileWithNotWavAndMp3ExtensionsOrLessThen256Bytes_DoesNotCollectThisFiles()
        {
            var alarm = new Alarm( new MediaPlayerFactory() );

            alarm.LoadDefaultSounds();

            Assert.That( alarm.DefaultSounds.Count, Is.EqualTo( 6 ) );
        }
    }
}
