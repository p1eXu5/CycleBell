using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Context
{
    public interface IAlarm {
        IPlayer Player { get; }
        IPlayer DefaultPlayer { get; }
        void SetDefaultSound ( string path );
        void AddSound ( TimePoint tPoint );
        void LoadSound ( TimePoint tPoint );
        void Play ( TimePoint nextTimePoint = null );
        void Stop ();
    }

    public class Alarm : IAlarm
    {
        private readonly IDictionary< int, Uri > _soundMap = new Dictionary< int, Uri >();

        public Alarm ( IPlayer player ) 
        {
            Player = player;
            DefaultPlayer = Player.ClonePlayer();
        }

        public IPlayer Player { get; }
        public IPlayer DefaultPlayer { get; }

        public void SetDefaultSound ( string path ) 
        {
            _soundMap[0] = new Uri( path );
            LoadSound( 0, DefaultPlayer );
        }

        public void AddSound ( TimePoint tPoint )
        {
            if ( tPoint == null ) throw new ArgumentNullException(nameof(tPoint), "TimePoint cannot be null.");
            if ( tPoint.Id == 0 
                 || tPoint.Tag == null 
                 || !(tPoint.Tag is string path) 
                 || string.IsNullOrWhiteSpace( path ) ) { throw new ArgumentException($"TimePoint was not correct. Id: {tPoint.Id}; Tag: {tPoint.Tag}.", nameof( tPoint )); }

            _soundMap[ tPoint.Id ] = new Uri( path );
        }

        public void LoadSound ( TimePoint tPoint )
        {
            if ( tPoint == null ) {
                LoadSound( 0, Player );
                return;
            }

            LoadSound( tPoint.Id, Player );
        }

        public void Play ( TimePoint nextTimePoint = null )
        {
            if ( nextTimePoint == null ) {
                if ( DefaultPlayer.HasAudio ) {
                    DefaultPlayer.Play();
                }
                return;
            }

            var nextSoundKey = nextTimePoint?.Id ?? 0;

            if ( Player.HasAudio ) {
                Player.Play();
                LoadSound( nextSoundKey, Player );
            }
        }

        public void Stop ()
        {
            Player.Stop();
            DefaultPlayer.Stop();
        }

        private void LoadSound ( int key, IPlayer player )
        {
            while ( true ) {
                if ( _soundMap.ContainsKey( key ) && player.Source?.Equals( _soundMap[ key ] ) != true ) {
                    player.Open( _soundMap[ key ] );
                }
                else {
                    if ( key != 0 ) {
                        key = 0;
                        continue;
                    }
                }

                break;
            }
        }
    }
}
