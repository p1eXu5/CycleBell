using System;
using System.Collections.Generic;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    public class Alarm : IAlarm
    {
        private readonly IDictionary< int, Uri > _soundMap = new Dictionary< int, Uri >();
        private IPlayer _playerB;

        public Alarm ( IPlayer player ) 
        {
            Player = player;
            _playerB = Player.ClonePlayer();
            DefaultPlayer = Player.ClonePlayer();
        }

        public IPlayer Player { get; private set; }
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

        public void Play ()
        {
            if ( Player.HasAudio ) {

                Player.Play();

                var tmp = Player;
                Player = _playerB;
                _playerB = tmp;
            }
        }

        public void PlayDefault ()
        {
            if ( DefaultPlayer.HasAudio ) {
                DefaultPlayer.Play();
            }
        }

        public void Stop ()
        {
            Player.Stop();
            _playerB.Stop();
        }

        public void StopDefault ()
        {
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
