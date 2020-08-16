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

            Assert.IsTrue( alarm.DefaultSoundsDirectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryDoesNotExist_DoesNotChangeDefaultDirectory()
        {
            var alarm = GetAlarm();

            alarm.DefaultSoundsDirectory = "Not existed directory";

            Assert.IsTrue( alarm.DefaultSoundsDirectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryExists_ChangesDefaultDirectory()
        {
            var alarm = GetAlarm();
            var debugDir = AppDomain.CurrentDomain.BaseDirectory;

            alarm.DefaultSoundsDirectory = debugDir;

            Assert.IsTrue( alarm.DefaultSoundsDirectory.Equals( debugDir) );
        }

        [ Test ]
        public void DefaultSoundsDirectory_NewDirectoryIsNull_ResetsDefaultDirectory()
        {
            var alarm = GetAlarm();
            var debugDir = AppDomain.CurrentDomain.BaseDirectory;

            alarm.DefaultSoundsDirectory = debugDir;
            Assert.IsTrue( alarm.DefaultSoundsDirectory.Equals( debugDir) );

            alarm.DefaultSoundsDirectory = null;
            Assert.IsTrue( alarm.DefaultSoundsDirectory.Equals( AppDomain.CurrentDomain.BaseDirectory + Alarm.BASE_SOUND_DIRECTORY ) );
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
            alarm.DefaultSoundsDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\..\\";

            alarm.LoadDefaultSoundCollection();

            Assert.That(alarm.DefaultSoundCollection.Count, Is.EqualTo(0));
        }


        #endregion


        #region SetDefaultSound( Source ) tests

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

        // TODO: when next sound is loaded and equals to default - reopen next
        [ Test ]
        public void SetDefaultSound_NativeUri_SetsDefaultSound()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();

            var uri = new Uri( AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 2.mp3" );
            var uri2 = new Uri( uri.ToString() );

            alarm.SetDefaultSound( uri2 );

            Assert.That( alarm.GetDefaultSound(), Is.EqualTo( uri2 ) );
        }



        #endregion


        #region SetDefaultSound tests

        [ Test ]
        public void SetDefaultSound_DefaultSoundCollectionEmpty_DoesNotSetDefaultSound()
        {
            var alarm = GetAlarm();

            alarm.SetDefaultSound();

            _mockDefaultPlayer.Verify( p => p.Open( It.IsAny<Uri>() ), Times.Never );
        }


        [ Test ]
        public void SetDefaultSound_DefaultSoundCollectionHasLoaded_SetsDefaultSound()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();

            alarm.SetDefaultSound();

            _mockDefaultPlayer.Verify( p => p.Open( It.IsAny<Uri>() ), Times.Once );
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


        #region LoadNextSound tests

        [Test]
        public void LoadNextSound__TimePointTagIsNull_DefaultSoundNotLoaded__CannotPlay()
        {
            var alarm = GetAlarm();

            alarm.LoadNextSound( null );

            Assert.IsFalse( alarm.CanPlay );
        }

        [Test]
        public void LoadNextSound__TimePointDoesNotInDictionary_DefaultSoundNotLoaded__CannotPlay()
        {
            var alarm = GetAlarm();

            alarm.LoadNextSound( new TimePoint() );

            Assert.IsFalse( alarm.CanPlay );
        }



        [Test]
        public void LoadNextSound__TimePointIsInDictionary__CanPlay()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );

            alarm.LoadNextSound( _timePointsWithSound[0] );

            Assert.IsTrue( alarm.CanPlay );
        }


        [Test]
        public void LoadNextSound__TimePointIsInDictionary_PlayerAIsNotSetted_CallsPlayerAOpen()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );

            alarm.LoadNextSound( _timePointsWithSound[0] );

            _mockPlayerA.Verify( p => p.Open( It.IsAny<Uri>() ), Times.Once );
        }

        [Test]
        public void LoadNextSound__TimePointIsInDictionary__SetsPlayerAHasAudioToTrue()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );

            alarm.LoadNextSound( _timePointsWithSound[0] );

            Assert.IsTrue( _mockPlayerA.Object.HasAudio );
        }


        [Test]
        public void LoadNextSound__TimePointTagIsNull_DefaultSoundHasLoaded__PlayerAIsDefaultPlayer()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();

            alarm.LoadNextSound( null );

            Assert.That( _playerDataDict[ _mockPlayerA ].Source, Is.EqualTo( _playerDataDict[ _mockPlayerA ].Source ) );
        }

        [Test]
        public void LoadNextSound__TimePointDoesNotInDictionary_DefaultSoundHasLoaded__PlayerAIsDefaultPlayer()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();

            alarm.LoadNextSound( new TimePoint() );

            Assert.That( _playerDataDict[ _mockPlayerA ].Source, Is.EqualTo( _playerDataDict[ _mockPlayerA ].Source ) );
        }


        [Test]
        public void LoadNextSound__TimePointIsInDictionary_PlayerAIsSettedLast_CallsPlayerBOpen()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.AddSound( _timePointsWithSound[1] );

            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[1] );

            _mockPlayerB.Verify( p => p.Open( It.IsAny<Uri>() ), Times.Once );
        }


        [Test]
        public void LoadNextSound__TimePointIsInDictionary_PlayerBIsSettedLast_CallsPlayerAOpen()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.AddSound( _timePointsWithSound[1] );
            alarm.AddSound( _timePointsWithSound[2] );

            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[1] );
            alarm.LoadNextSound( _timePointsWithSound[2] );

            _mockPlayerA.Verify( p => p.Open( It.IsAny<Uri>() ), Times.Exactly( 2 ) );
        }

        #endregion


        #region Play tests

        [ Test ]
        public void Play_ByDefault_DoesNotCallPlayersPlay()
        {
            var alarm = GetAlarm();

            alarm.Play();

            _mockDefaultPlayer.Verify( p => p.Play(), Times.Never() );
            _mockPlayerA.Verify( p => p.Play(), Times.Never() );
            _mockPlayerB.Verify( p => p.Play(), Times.Never() );
        }

        [Test]
        public void Play_CurrentPlayerNotSet_DoesNotCallPlayersPlayMethod()
        {
            var alarm = GetAlarm();

            alarm.LoadNextSound( null );
            alarm.Play();

            _mockDefaultPlayer.Verify( p => p.Play(), Times.Never() );
            _mockPlayerA.Verify( p => p.Play(), Times.Never() );
            _mockPlayerB.Verify( p => p.Play(), Times.Never() );
        }

        [Test]
        public void Play_TimePointIsInDictionary_PlayPlayerA()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );

            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.Play();

            _mockPlayerA.Verify( p => p.Play(), Times.Once );
        }

        [ Test ]
        public void Play_NextPlayerIsPlayerA_CallsPlayerBStop()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[0] );

            alarm.Play();

            _mockPlayerB.Verify( p => p.Stop(), Times.Once );
        }

        [ Test ]
        public void Play_NextPlayerIsPlayerB_CallsPlayerAStop()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.AddSound( _timePointsWithSound[1] );
            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[1] );

            alarm.Play();

            _mockPlayerA.Verify( p => p.Stop(), Times.Once );
        }

        #endregion


        #region PlayDefault tests

        [ Test ]
        public void PlayDefault_ByDefault_DoesNotCallPlayersPlay()
        {
            var alarm = GetAlarm();

            alarm.PlayDefault();

            _mockDefaultPlayer.Verify( p => p.Play(), Times.Never() );
            _mockPlayerA.Verify( p => p.Play(), Times.Never() );
            _mockPlayerB.Verify( p => p.Play(), Times.Never() );
        }

        [ Test ]
        public void PlayDefault_DefaultSoundSetted_CallsDefaultPlayerPlay()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();
            alarm.SetDefaultSound();

            alarm.PlayDefault();

            _mockDefaultPlayer.Verify( p => p.Play(), Times.Once() );
            _mockPlayerA.Verify( p => p.Play(), Times.Never() );
            _mockPlayerB.Verify( p => p.Play(), Times.Never() );
        }

        #endregion


        #region CanPlayDefault tests

        [ Test ]
        public void CanPlayDefault_DyDefault_ReturnsFalse()
        {
            var alarm = GetAlarm();

            Assert.IsFalse( alarm.CanPlayDefault );
        }

        [ Test ]
        public void CanPlayDefault_DefaultSoundIsSetted_ReturnsFalse()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();
            alarm.SetDefaultSound();

            Assert.IsTrue( alarm.CanPlayDefault );
        }

        #endregion


        #region Stop tests

        [ Test ]
        public void Stop_ByDefault_DoesNotCallsPlayersStopMethod()
        {
            var alarm = GetAlarm();

            alarm.Stop();

            _mockDefaultPlayer.Verify( p => p.Stop(), Times.Never );
            _mockPlayerA.Verify( p => p.Stop(), Times.Never );
            _mockPlayerB.Verify( p => p.Stop(), Times.Never );
        }

        [ Test ]
        public void Stop_FirstTimePointSoundIsSetted_CallsPlayerAStopMethod()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[0] );

            alarm.Stop();

            _mockPlayerA.Verify( p => p.Stop(), Times.Once );
        }

        [ Test ]
        public void Stop_TwoTimePointSoundIsSetted_CallsPlayersStopMethod()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.AddSound( _timePointsWithSound[1] );
            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[1] );

            alarm.Stop();

            _mockPlayerA.Verify( p => p.Stop(), Times.Once );
            _mockPlayerB.Verify( p => p.Stop(), Times.Once );
        }

        #endregion


        #region StopDefault tests

        [ Test ]
        public void StopDefault_ByDefault_DoesNotCallsPlayersStopMethod()
        {
            var alarm = GetAlarm();

            alarm.StopDefault();

            _mockDefaultPlayer.Verify( p => p.Stop(), Times.Never );
            _mockPlayerA.Verify( p => p.Stop(), Times.Never );
            _mockPlayerB.Verify( p => p.Stop(), Times.Never );
        }

        [ Test ]
        public void StopDefault_DefaultSoundIsSetted_CallsDefaultPlayerStopMethod()
        {
            var alarm = GetAlarm();
            alarm.LoadDefaultSoundCollection();
            alarm.SetDefaultSound();

            alarm.StopDefault();

            _mockDefaultPlayer.Verify( p => p.Stop(), Times.Once );
            _mockPlayerA.Verify( p => p.Stop(), Times.Never );
            _mockPlayerB.Verify( p => p.Stop(), Times.Never );
        }

        #endregion


        #region Reset tests

        [ Test ]
        public void Reset_NextPleyerIsLoaded_SetsNextPlayerToNull()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[0] );

            alarm.Reset();
            alarm.Play();

            _mockPlayerA.Verify( p => p.Play(), Times.Never );
            _mockPlayerB.Verify( p => p.Play(), Times.Never );
            _mockDefaultPlayer.Verify( p => p.Play(), Times.Never );
        }

        #endregion



        #region RemoveSound tests

        [ Test ]
        public void RemoveSound_SoundIsNotSetted_CanRemoveSound()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );

            Assert.That( alarm.TimePointSoundDictionary.Count(), Is.EqualTo( 1 ) );

            alarm.RemoveSound( _timePointsWithSound[ 0 ] );

            Assert.That( alarm.TimePointSoundDictionary.Count(), Is.EqualTo( 0 ) );
        }

        [ Test ]
        public void RemoveSound_SoundIsSettedInNextPlayerA_ClosePlayerA()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[0] );

            alarm.RemoveSound( _timePointsWithSound[ 0 ] );

            _mockPlayerA.Verify( p => p.Close(), Times.Once );
        }

        [ Test ]
        public void RemoveSound_SoundIsSettedInNextPlayerB_ClosePlayerB()
        {
            var alarm = GetAlarm();
            alarm.AddSound( _timePointsWithSound[0] );
            alarm.AddSound( _timePointsWithSound[1] );
            alarm.LoadNextSound( _timePointsWithSound[0] );
            alarm.LoadNextSound( _timePointsWithSound[1] );

            alarm.RemoveSound( _timePointsWithSound[ 1 ] );

            _mockPlayerB.Verify( p => p.Close(), Times.Once );
        }

        #endregion


        #region fields

        private Dictionary< Mock< IPlayer >, PlayerData > _playerDataDict;
        private Mock< IPlayer > _mockDefaultPlayer;
        private Mock< IPlayer > _mockPlayerA;
        private Mock< IPlayer > _mockPlayerB;

        private readonly TimePoint[] _timePointsWithSound = new [] {
            new TimePoint { Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Alarm 666.mp3" },
            new TimePoint { Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 1.mp3" },
            new TimePoint { Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 2.mp3" },
            new TimePoint { Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 3.mp3" },
        };

        #endregion


        #region factories

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
            mockPlayer.Setup( p => p.HasAudio ).Returns( () => _playerDataDict[ mockPlayer ].HasAudio );
            mockPlayer.Setup( p => p.Source ).Returns( () => _playerDataDict[ mockPlayer ].Source );
            mockPlayer.Setup( p => p.Play() ).Callback( () => _playerDataDict[ mockPlayer ].Play() );
            mockPlayer.Setup( p => p.Stop() ).Callback( () => _playerDataDict[ mockPlayer ].Stop() );
            mockPlayer.Setup( p => p.Stop() ).Callback( () => _playerDataDict[ mockPlayer ].Stop() );

            return mockPlayer;
        }

        #endregion


        #region types

        private class PlayerData
        {
            private bool _isPlaying;

            public Uri Source { get; private set; }

            public bool HasAudio => Source != null;

            public bool IsPlaying => HasAudio && _isPlaying;

            public void Open( Uri uri )
            {
                Source = uri;
            }

            public void Close()
            {
                Source = null;
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
