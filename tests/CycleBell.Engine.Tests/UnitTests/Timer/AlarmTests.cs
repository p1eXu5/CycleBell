using System;
using System.Collections.Generic;
using System.Linq;
using CycleBell.Engine.Models;
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
            Assert.That( ex.Message, Is.EqualTo( "Player factory cannot be null.\r\nParameter name: playerFactory"));
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
        public void LoadDefaultSoundCollection_FolderIsEmpty_DoesNotLoadDefaultSoundCollection()
        {
            var alarm = GetAlarm();
            alarm.DefaultSoundsDirrectory = AppDomain.CurrentDomain.BaseDirectory + "\\..\\";

            alarm.LoadDefaultSoundCollection();

            Assert.That(alarm.DefaultSoundCollection.Count, Is.EqualTo(0));
        }


        #endregion


        #region SetDefaultSound tests

        [ Test ]
        public void SetDefaultSound_UriIsNull_DoesNotSetDefaultSound()
        {
            // Arrange:
            var alarm = GetAlarm();

            // Action:
            alarm.SetDefaultSound( null );

            // Assert:
            Assert.IsFalse( _playerDataDict[ _mockDefaultPlayer ].HasAudio );
        }

        [ Test ]
        public void SetDefaultSound_FileNotExist_DoesNotSetDefaultSound()
        {
            // Arrange:
            var alarm = GetAlarm();
            var uri = new Uri( "Sounds/Alarm N.mp3", UriKind.Relative );

            // Action:
            alarm.SetDefaultSound( uri );

            // Assert:
            Assert.IsFalse( _playerDataDict[ _mockDefaultPlayer ].HasAudio );
        }

        [ Test ]
        public void SetDefaultSound_DefaultSoundsCollectionDoesNotContainUri_AddsUriToDefaultSoundsCollection()
        {
            // Arrange:
            var alarm = GetAlarm();
            var uri = new Uri( AppDomain.CurrentDomain.BaseDirectory + "\\Alarm 666.mp3", UriKind.Absolute );

            // Action:
            alarm.SetDefaultSound( uri );

            // Assert:
            Assert.IsTrue( alarm.DefaultSoundCollection.Contains( uri )  );
        }

        #endregion


        #region TimePointSoundDictionary test

        [ Test ]
        public void TimePointSoundDictionary_ByDefault_IsEmpty()
        {
            var alarm = GetAlarm();

            Assert.That( alarm.TimePointSoundDictionary, Is.Empty );
        }

        #endregion


        #region AddSound tests

        [ Test ]
        public void AddSound_TimePointTagIsNull_DoesNotAddUri()
        {
            var alarm = GetAlarm();

            alarm.AddSound( null );

            Assert.That( alarm.TimePointSoundDictionary, Is.Empty );
        }

        [ Test ]
        public void AddSound_TimePointTagIsValidUrl_AddsUri()
        {
            var alarm = GetAlarm();
            var uri = new Uri( AppDomain.CurrentDomain.BaseDirectory + "\\Alarm 666.mp3" );

            alarm.AddSound( new TimePoint() { Tag = uri.OriginalString } );

            Assert.That( alarm.TimePointSoundDictionary.First().Value, Is.EqualTo( uri ) );
        }

        #endregion


        #region NextPlayer tests

        [ Test ]
        public void NextPlayer_ByDefault_IsNull()
        {
            var alarm = GetAlarm();

            Assert.IsNull( alarm.NextPlayer );
        }

        #endregion


        #region LoadNextSound tests

        

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
