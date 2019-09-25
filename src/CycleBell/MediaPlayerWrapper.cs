using System;
using System.Windows;
using System.Windows.Media;
using CycleBell.Engine.Timer;

namespace CycleBell
{
    public class MediaPlayerWrapper : IPlayer
    {
        private readonly MediaPlayer _player = new MediaPlayer();
        public Uri Source => _player.Source;
        public bool HasAudio => _player.HasAudio;
        public void Open( Uri source ) => _player.Open( source );
        public void Play() => _player.Play();
        public void Stop() => _player.Stop();
        public void Close() => _player.Close();

        public double BufferingProgress => _player.BufferingProgress;
        public bool IsBuffering => _player.IsBuffering;
        public Duration NaturalDuration => _player.NaturalDuration;

        public event EventHandler MediaOpened
        {
            add => _player.MediaOpened += value;
            remove => _player.MediaOpened -= value;
        }

        public event EventHandler MediaEnded
        {
            add => _player.MediaEnded += value;
            remove => _player.MediaEnded -= value;
        }
    }
}
