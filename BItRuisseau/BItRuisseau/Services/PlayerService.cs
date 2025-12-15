using BitRuisseau.Models;
using BitRuisseau.Protocol;
using NAudio.Wave;
using System.Diagnostics;
using System.Timers;

namespace BitRuisseau.Services
{
    public class PlayerService : IDisposable
    {
        public List<MediaDescription> Playlist { get; set; } = new();

        private MediaDescription? _currentSong;
        public MediaDescription? CurrentSong
        {
            get => _currentSong;
            private set
            {
                if (_currentSong != value)
                {
                    _currentSong = value;
                    OnSongChanged?.Invoke();
                }
            }
        }

        private IWavePlayer? waveOut;
        private AudioFileReader? audioFile;

        private readonly System.Timers.Timer _timer;

        public event Action? OnTimeUpdated;
        public event Action? OnSongChanged;

        public TimeSpan Elapsed =>
            audioFile?.CurrentTime ?? TimeSpan.Zero;

        public bool IsPlaying => waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => waveOut?.PlaybackState == PlaybackState.Paused;
        public bool IsStopped => waveOut == null || waveOut.PlaybackState == PlaybackState.Stopped;

        public PlayerService()
        {
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += (_, _) => OnTimeUpdated?.Invoke();
        }

        public void SetPlaylist(List<MediaDescription> playlist)
        {
            Playlist = playlist ?? new List<MediaDescription>();
        }

        public void Play(MediaDescription song)
        {
            if (song.FilePath == null) return;
            if (song == null)
                return;

            if (IsPaused && CurrentSong == song)
            {
                waveOut?.Play();
                _timer.Start();
                return;
            }

            if (IsPlaying && CurrentSong == song)
                return;

            Stop();

            CurrentSong = song;
            audioFile = new AudioFileReader(song.FilePath);
            waveOut = new WaveOutEvent();
            
            waveOut.PlaybackStopped += OnPlaybackStopped;
            waveOut.Init(audioFile);
            waveOut.Play();

            _timer.Start();
        }

        public void Pause()
        {
            if (!IsPlaying)
                return;

            waveOut?.Pause();
            _timer.Stop();
        }

        public void Stop()
        {
            _timer.Stop();

            if (waveOut != null)
            {
                waveOut.PlaybackStopped -= OnPlaybackStopped;
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            audioFile?.Dispose();
            audioFile = null;
        }

        public void PlayNext()
        {
            if (CurrentSong == null || Playlist.Count == 0)
                return;

            var index = Playlist.IndexOf(CurrentSong);
            if (index >= 0 && index < Playlist.Count - 1)
                Play(Playlist[index + 1]);
        }

        public void PlayPrevious()
        {
            if (CurrentSong == null || Playlist.Count == 0)
                return;

            var index = Playlist.IndexOf(CurrentSong);
            if (index > 0)
                Play(Playlist[index - 1]);
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (audioFile != null && audioFile.Position >= audioFile.Length)
            {
                PlayNext();
            }
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
        }
    }
}
