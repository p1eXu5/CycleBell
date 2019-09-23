using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    public class Alarm : IAlarm
    {
        #region const

        public const string BASE_SOUND_DIRECTORY = "\\Sounds";

        #endregion


        #region fields

        private readonly string[] _filter = new[] { ".mp3", ".wav" };

        private string _defaultSoundsDirectory;

        private readonly ObservableCollection<Uri> _defaultSounds;
        private readonly IDictionary<int, Uri> _soundMap = new Dictionary<int, Uri>();

        private readonly IPlayer _defaultPlayer;
        private readonly IPlayer _playerA;
        private readonly IPlayer _playerB;
        private IPlayer _currentPlayer;

        private readonly Uri _defaultSound;

        #endregion


        #region ctor

        public Alarm ( IPlayerFactory playerFactory, Uri defaultSound = null ) 
        {
            if (playerFactory == null) throw new ArgumentNullException(nameof(playerFactory), @"Player factory cannot be null.");

            _defaultSound = defaultSound;

            _defaultPlayer = playerFactory.CreatePlayer();

            _playerA = playerFactory.CreatePlayer();
            _playerB = playerFactory.CreatePlayer();

            _defaultSounds = new ObservableCollection< Uri >();
            DefaultSounds = new ReadOnlyObservableCollection< Uri >( _defaultSounds );
        }

        #endregion


        #region properties

        public ReadOnlyObservableCollection< Uri > DefaultSounds { get; }

        public string DefaultSoundsDirrectory 
            => String.IsNullOrWhiteSpace( _defaultSoundsDirectory )
                     ? (_defaultSoundsDirectory = AppDomain.CurrentDomain.BaseDirectory + BASE_SOUND_DIRECTORY)
                     : _defaultSoundsDirectory;

        #endregion


        #region public methods

        /// <summary>
        /// Fills DefaultSounds collection, set up default player.
        /// </summary>
        public void LoadDefaultSounds()
        {
            bool IsValid( string fileName )
            {
                bool isValid = false;

                foreach ( var extension in _filter ) {
                    if ( fileName.EndsWith( extension ) ) {
                        isValid = true;
                        break;
                    }
                }

                if ( !isValid ) return false;

                FileInfo fi;
                try {
                    fi = new FileInfo( fileName );
                }
                catch ( SecurityException ) {
                    return false;
                }
                catch ( UnauthorizedAccessException ) {
                    return false;
                }
                catch ( PathTooLongException ) {
                    return false;
                }
                
                if ( fi.Length < 256 ) return false;

                return true;
            }

            _defaultSounds.Clear();
            

            var defaultSoundsDirectory = DefaultSoundsDirrectory;

            if ( Directory.Exists( defaultSoundsDirectory ) ) {

                foreach ( var fileName in Directory.EnumerateFiles( defaultSoundsDirectory, "*.*", SearchOption.TopDirectoryOnly ) ) {

                    if ( !IsValid( fileName ) ) continue;
                    _defaultSounds.Add( new Uri( fileName ) );
                }
            }

            if ( _defaultSound != null && IsValid( _defaultSound.LocalPath ) ) {
                SetDefaultSound( _defaultSound );
            }
            else if ( DefaultSounds.Count > 0) {
                SetDefaultSound( DefaultSounds.First() );
            }
        }

        public bool SetDefaultSound( Uri uri )
        {
            if ( uri == null ) return false;

            if ( !_defaultSounds.Contains( uri ) ) {
                _defaultSounds.Add( uri );
            }

            if ( _defaultPlayer.Source != null && _defaultPlayer.Source.Equals( uri ) ) {
                return true;
            }

            _defaultPlayer.Stop();
            _defaultPlayer.Close();
            _defaultPlayer.Open( uri );

            return true;
        }

        /// <summary>
        /// Must be called when the preset is being loaded
        /// </summary>
        /// <param name="tPoint"></param>
        public void AddSound ( TimePoint tPoint )
        {
            if ( tPoint == null
                 || tPoint.Id == 0 
                 || tPoint.Tag == null 
                 || !(tPoint.Tag is string path) 
                 || string.IsNullOrWhiteSpace( path ) ) return;

            Uri uri;

            try {
                uri = new Uri( path );
            }
            catch ( UriFormatException ) {
                return;
            }

            _soundMap[ tPoint.Id ] = uri;
        }

        public void LoadNextSound ( TimePoint timePoint )
        {
            if ( timePoint == null || !_soundMap.ContainsKey( timePoint.Id ) ) {
                _currentPlayer = _defaultPlayer;
            }
            else {
                if ( _currentPlayer == _playerA ) {
                    _playerB.Open( _soundMap[ timePoint.Id ] );
                    _currentPlayer = _playerB;
                }
                else {
                    _playerA.Open( _soundMap[ timePoint.Id ] );
                    _currentPlayer = _playerA;
                }
            }
        }

        public void Play ()
        {
            if ( _currentPlayer != null && _currentPlayer.HasAudio ) {
                _currentPlayer.Play();
            }
        }

        public void PlayDefault()
        {
            if ( _defaultPlayer != null && _defaultPlayer.HasAudio ) {
                _defaultPlayer.Play();
            }
        }

        public void Stop()
        {
            if ( _currentPlayer != null && _currentPlayer.HasAudio ) {
                _currentPlayer.Stop();
            }
        }

        public void StopDefault ()
        {
            if ( _defaultPlayer != null && _defaultPlayer.HasAudio ) {
                _defaultPlayer.Stop();
            }
        }

        #endregion
    }
}
