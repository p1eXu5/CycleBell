using System;
using System.Collections.Generic;
using CycleBell.Engine.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.UnitTests.Timer
{
    [ TestFixture ]
    public class AlarmTests
    {
        #region ctor tests

        [Test]
        public void Ctor_PlayerFactoryIsNull_Throws()
        {
            var ex = Assert.Catch<ArgumentNullException>(() => new Alarm(null));
        }

        #endregion


        #region DefaultSoundsDirectory tests

        [ Test ]
        public void DefaultSoundsDirectory_ByDefault_EqualsSoundsDirectory()
        {
            var alarm = GetAlarm();

            Assert.IsTrue( alarm.DefaultSoundsDirrectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryDoesNotExist_DoesNotChangeDefaultDirectory()
        {
            var alarm = GetAlarm();

            alarm.DefaultSoundsDirrectory = "Not existed directory";

            Assert.IsTrue( alarm.DefaultSoundsDirrectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryExists_ChangesDefaultDirectory()
        {
            var alarm = GetAlarm();
            var debugDir = AppDomain.CurrentDomain.BaseDirectory;

            alarm.DefaultSoundsDirrectory = debugDir;

            Assert.IsTrue( alarm.DefaultSoundsDirrectory.Equals( debugDir) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryIsNull_ResetsDefaultDirectory()
        {
            var alarm = GetAlarm();
            var debugDir = AppDomain.CurrentDomain.BaseDirectory;

            alarm.DefaultSoundsDirrectory = debugDir;
            Assert.IsTrue( alarm.DefaultSoundsDirrectory.Equals( debugDir) );

            alarm.DefaultSoundsDirrectory = null;
            Assert.IsTrue( alarm.DefaultSoundsDirrectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
        }

        #endregion


        #region LoadDefaultSoundCollection

        [ Test ]
        public void LoadDefaultSoundCollection_FolderContainsFileWithNotWavAndMp3ExtensionsOrLessThen256Bytes_DoesNotCollectThisFiles()
        {
            var alarm = GetAlarm();

            alarm.LoadDefaultSoundCollection();

            Assert.That(alarm.DefaultSoundCollection.Count, Is.EqualTo(6));
        }

        [ Test ]
        public void LoadDefaultSoundCollection_FolderIsEmpty_ReturnsEmptyDefaultSoundCollection()
        {
            var alarm = GetAlarm();
            alarm.DefaultSoundsDirrectory = AppDomain.CurrentDomain.BaseDirectory;

            alarm.LoadDefaultSoundCollection();

            Assert.That(alarm.DefaultSoundCollection.Count, Is.EqualTo(0));
        }

        [ Test ]
        public void LoadDefaultSoundCollection_FolderContainsSounds_LoadDefaultPlayer()
        {

        }

        #endregion


        #region test fields

        private Dictionary< Mock< IPlayer >, PlayerData > _playerDataDict;
        private Mock< IPlayer > _mockDefaultPlayer;
        private Mock< IPlayer > _mockPlayerA;
        private Mock< IPlayer > _mockPlayerB;

        #endregion


        #region factory

        private IAlarm GetAlarm()
        {
            return new Alarm( GetPlayerFactory() );
        }

        /// <summary>
        /// Returns mocked playeer factory.
        /// </summary>
        /// <returns></returns>
        private IPlayerFactory GetPlayerFactory()
        {
            _mockDefaultPlayer = GetPlayer();
            _mockPlayerA = GetPlayer();
            _mockPlayerB = GetPlayer();

            var mockPlayerFactory = new Mock< IPlayerFactory >();

            mockPlayerFactory.SetupSequence( p => p.CreatePlayer() )
                             .Returns( _mockDefaultPlayer.Object )
                             .Returns( _mockPlayerA.Object )
                             .Returns( _mockPlayerB.Object );

            return mockPlayerFactory.Object;
        }

        private Mock< IPlayer > GetPlayer()
        {
            if ( _playerDataDict == null ) {
                _playerDataDict = new Dictionary< Mock<IPlayer>, PlayerData>(3);
            }

            var mockPlayer = new Mock< IPlayer >();
            _playerDataDict[ mockPlayer ] = new PlayerData();

            mockPlayer.Setup( p => p.Open( It.IsAny< Uri >() ) ).Callback< Uri >( u => _playerDataDict[ mockPlayer ].Open( u ) );
            mockPlayer.Setup( p => p.Close() ).Callback( () => _playerDataDict[ mockPlayer ].Close() );
            mockPlayer.Setup( p => p.HasAudio ).Returns( _playerDataDict[ mockPlayer ].HasAudio );
            mockPlayer.Setup( p => p.Play() ).Callback( () => _playerDataDict[ mockPlayer ].Play() );
            mockPlayer.Setup( p => p.Stop() ).Callback( () => _playerDataDict[ mockPlayer ].Stop() );

            return mockPlayer;
        }

        #endregion


        #region test types

        private struct PlayerData
        {
            private bool _isPlaying;

            public Uri Uri { get; private set; }

            public bool HasAudio => Uri != null;

            public bool IsPlaying => HasAudio && _isPlaying;

            public void Open( Uri uri )
            {
                Uri = uri;
            }

            public void Close()
            {
                Uri = null;
            }

            public void Play()
            {
                _isPlaying = true;
            }

            public void Stop()
            {
                _isPlaying = false;
            }
        }

        #endregion
    }
}
