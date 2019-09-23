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

        private readonly ObservableCollection<Uri> _defaultSoundCollection;
        private readonly IDictionary<int, Uri> _soundMap = new Dictionary<int, Uri>();

        private readonly IPlayer _defaultPlayer;
        private readonly IPlayer _playerA;
        private readonly IPlayer _playerB;


        #endregion


        #region ctor

        public Alarm ( IPlayerFactory playerFactory ) 
        {
            if (playerFactory == null) throw new ArgumentNullException(nameof(playerFactory), @"Player factory cannot be null.");

            _defaultPlayer = playerFactory.CreatePlayer();

            _playerA = playerFactory.CreatePlayer();
            _playerB = playerFactory.CreatePlayer();

            _defaultSoundCollection = new ObservableCollection< Uri >();
            DefaultSoundCollection = new ReadOnlyObservableCollection< Uri >( _defaultSoundCollection );
        }

        #endregion


        #region properties

        public ReadOnlyObservableCollection< Uri > DefaultSoundCollection { get; }

        /// <summary>
        /// When assigned null resets default directory to <see cref="BASE_SOUND_DIRECTORY"/>
        /// </summary>
        public string DefaultSoundsDirrectory
        {
            get => String.IsNullOrWhiteSpace( _defaultSoundsDirectory )
                       ? (_defaultSoundsDirectory = AppDomain.CurrentDomain.BaseDirectory + BASE_SOUND_DIRECTORY)
                       : _defaultSoundsDirectory;

            set {
                if ( value == null || Directory.Exists( value ) ) {
                    _defaultSoundsDirectory = value;
                }
            }
        }

        public IEnumerable< KeyValuePair< int, Uri > > TimePointSoundDictionary => _soundMap;
        
        public IPlayer NextPlayer { get; private set; }

        #endregion


        #region public methods

        /// <summary>
        /// Fills DefaultSoundCollection collection, set up default player.
        /// </summary>
        public void LoadDefaultSoundCollection()
        {
            var defaultSoundsDirectory = DefaultSoundsDirrectory;
            var uriList = new List< Uri >();

            if ( Directory.Exists( defaultSoundsDirectory ) ) {

                foreach ( var fileName in Directory.EnumerateFiles( defaultSoundsDirectory, "*.*", SearchOption.TopDirectoryOnly ) ) {

                    if ( !IsValid( fileName ) ) continue;
                    uriList.Add( new Uri( fileName ) );
                }
            }

            if ( uriList.Any() ) 
            {
                _defaultSoundCollection.Clear();
                
                foreach ( var uri in uriList ) {
                    _defaultSoundCollection.Add( uri );
                }
            }
        }

        public void SetDefaultSound()
        {
            if ( DefaultSoundCollection.Count > 0) {
                SetDefaultSound( DefaultSoundCollection.First() );
            }
        }

        public void SetDefaultSound( Uri uri )
        {
            if ( uri == null || !IsValid( uri.OriginalString ) ) return;

            if ( !_defaultSoundCollection.Contains( uri ) ) {
                _defaultSoundCollection.Add( uri );
            }

            if ( _defaultPlayer.Source != null && _defaultPlayer.Source.Equals( uri ) ) {
                return;
            }

            _defaultPlayer.Stop();
            _defaultPlayer.Close();
            _defaultPlayer.Open( uri );
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
                 || !IsValid( path ) ) return;

            Uri uri;

            try {
                uri = new Uri( path );
            }
            catch ( UriFormatException ) {
                return;
            }

            _soundMap[ tPoint.Id ] = uri;
        }

        public void LoadNextSound( TimePoint timePoint )
        {
            if ( timePoint == null || !_soundMap.ContainsKey( timePoint.Id ) ) {
                NextPlayer = _defaultPlayer;
            }
            else {
                if ( NextPlayer == _playerA ) {
                    _playerB.Open( _soundMap[ timePoint.Id ] );
                    NextPlayer = _playerB;
                }
                else {
                    _playerA.Open( _soundMap[ timePoint.Id ] );
                    NextPlayer = _playerA;
                }
            }
        }

        public void Play()
        {
            if ( NextPlayer != null && NextPlayer.HasAudio ) {
                NextPlayer.Play();
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
            if ( NextPlayer != null && NextPlayer.HasAudio ) {
                NextPlayer.Stop();
            }
        }

        public void StopDefault ()
        {
            if ( _defaultPlayer != null && _defaultPlayer.HasAudio ) {
                _defaultPlayer.Stop();
            }
        }

        #endregion

        #region private methods

        private bool IsValid( string fileName )
        {
            bool isValid = false;

            foreach ( var extension in _filter ) {
                if ( fileName.EndsWith( extension ) ) {
                    isValid = true;
                    break;
                }
            }

            if ( !isValid ) return false;

            if ( !File.Exists( fileName ) ) return false;

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

        #endregion
    }
}
