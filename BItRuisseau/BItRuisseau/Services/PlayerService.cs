using BitRuisseau.Models;
using BitRuisseau.Models.Protocol;
using NAudio.Wave;
using System.Timers;

namespace BitRuisseau.Services;

/// <summary>
/// Service responsible for audio playback and playlist management.
/// </summary>
public class PlayerService : IDisposable
{
    private readonly System.Timers.Timer _timer;

    private IWavePlayer? _waveOut;
    private AudioFileReader? _audioFile;

    private MediaDescription? _currentSong;

    /// <summary>
    /// Current playlist.
    /// </summary>
    public List<MediaDescription> Playlist { get; private set; } = new();

    /// <summary>
    /// Currently playing song.
    /// </summary>
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

    /// <summary>
    /// Triggered when playback time is updated.
    /// </summary>
    public event Action? OnTimeUpdated;

    /// <summary>
    /// Triggered when the current song changes.
    /// </summary>
    public event Action? OnSongChanged;

    /// <summary>
    /// Elapsed playback time.
    /// </summary>
    public TimeSpan Elapsed =>
        _audioFile?.CurrentTime ?? TimeSpan.Zero;

    /// <summary>
    /// Indicates whether audio is currently playing.
    /// </summary>
    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

    public bool IsPaused => _waveOut?.PlaybackState == PlaybackState.Paused;

    public bool IsStopped =>
        _waveOut == null || _waveOut.PlaybackState == PlaybackState.Stopped;

    /// <summary>
    /// Initializes a new instance of the PlayerService.
    /// </summary>
    public PlayerService()
    {
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += (_, _) => OnTimeUpdated?.Invoke();
    }

    /// <summary>
    /// Sets the current playlist.
    /// </summary>
    public void SetPlaylist(List<MediaDescription> playlist)
    {
        Playlist = playlist ?? new List<MediaDescription>();
    }

    /// <summary>
    /// Plays the given song.
    /// </summary>
    public void Play(MediaDescription song)
    {
        if (song == null || song.FilePath == null)
            return;

        if (IsPaused && CurrentSong == song)
        {
            _waveOut?.Play();
            _timer.Start();
            return;
        }

        if (IsPlaying && CurrentSong == song)
            return;

        Stop();

        CurrentSong = song;

        _audioFile = new AudioFileReader(song.FilePath);
        _waveOut = new WaveOutEvent();

        _waveOut.PlaybackStopped += OnPlaybackStopped;
        _waveOut.Init(_audioFile);
        _waveOut.Play();

        _timer.Start();
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
    {
        if (!IsPlaying)
            return;

        _waveOut?.Pause();
        _timer.Stop();
    }

    /// <summary>
    /// Stops playback and releases resources.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();

        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        _audioFile?.Dispose();
        _audioFile = null;
    }

    /// <summary>
    /// Plays the next song in the playlist.
    /// </summary>
    public void PlayNext()
    {
        if (CurrentSong == null || Playlist.Count == 0)
            return;

        var index = Playlist.IndexOf(CurrentSong);
        if (index >= 0 && index < Playlist.Count - 1)
            Play(Playlist[index + 1]);
    }

    /// <summary>
    /// Plays the previous song in the playlist.
    /// </summary>
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
        if (_audioFile != null && _audioFile.Position >= _audioFile.Length)
        {
            PlayNext();
        }
    }

    /// <summary>
    /// Releases all resources used by the player.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _timer.Dispose();
    }
}
