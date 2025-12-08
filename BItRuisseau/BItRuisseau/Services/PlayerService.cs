using BItRuisseau.Models;
using NAudio.Wave;

namespace BItRuisseau.Services
{
    public class PlayerService : IDisposable
    {
        public List<Music> Playlist { get; set; } = new();
        private Music _currentSong;
        public Music CurrentSong
        {
            get => _currentSong;
            private set
            {
                _currentSong = value;
                OnSongChanged?.Invoke();
            }
        }

        private IWavePlayer waveOut;
        private AudioFileReader audioFile;

        public bool IsPlaying => waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => waveOut?.PlaybackState == PlaybackState.Paused;
        public bool IsStopped => waveOut?.PlaybackState == PlaybackState.Stopped || waveOut == null;

        public event Action OnSongChanged;

        public void SetPlaylist(List<Music> playlist) => Playlist = playlist;

        public void Play(Music song)
        {
            if (song == null) return;
            if (IsPlaying && CurrentSong == song) return;

            Stop();

            CurrentSong = song;
            audioFile = new AudioFileReader(song.FilePath);
            waveOut = new WaveOutEvent();
            waveOut.Init(audioFile);
            waveOut.Play();
        }

        public void Pause() => waveOut?.Pause();

        public void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                audioFile?.Dispose();
                waveOut.Dispose();
                audioFile = null;
                waveOut = null;
            }
        }

        public void PlayNext()
        {
            if (Playlist == null || CurrentSong == null) return;
            var index = Playlist.IndexOf(CurrentSong);
            if (index >= 0 && index < Playlist.Count - 1)
                Play(Playlist[index + 1]);
        }

        public void PlayPrevious()
        {
            if (Playlist == null || CurrentSong == null) return;
            var index = Playlist.IndexOf(CurrentSong);
            if (index > 0)
                Play(Playlist[index - 1]);
        }

        public void Dispose() => Stop();
    }
}
