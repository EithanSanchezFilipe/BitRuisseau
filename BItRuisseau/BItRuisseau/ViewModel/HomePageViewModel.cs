using BitRuisseau.Models;
using BitRuisseau.Models.Protocol;
using BitRuisseau.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BitRuisseau.ViewModel;

/// <summary>
/// Main ViewModel for the home page.
/// Handles music lists, player state and network communication.
/// </summary>
public class HomePageViewModel : IDisposable
{
    private readonly AgentService _agentService;
    private readonly MusicService _musicService;
    private readonly PlayerService _playerService;
    public MediaCenter MyMediaCenter { get; }
    public ObservableCollection<MediaDescription> MyMusicList { get; } = new();
    public ObservableCollection<MediaDescription> CurrentMusicList { get; } = new();


    // Event triggered when the player state changes.
    public event Action? OnPlayerChanged;

    /// <summary>
    /// Initializes a new instance of the HomePageViewModel.
    /// </summary>
    /// <param name="agentService">Network agent service</param>
    /// <param name="musicService">Music management service</param>
    /// <param name="playerService">Audio player service</param>
    /// <param name="localMediaCenter">Local media center service</param>
    public HomePageViewModel(
        AgentService agentService,
        MusicService musicService,
        PlayerService playerService,
        LocalMediaCenterService localMediaCenter)
    {
        _agentService = agentService;
        _musicService = musicService;
        _playerService = playerService;

        MyMediaCenter = localMediaCenter.Instance;

        _agentService.MediaListReceived += OnMediaListReceived;
        _agentService.CatalogRequested += OnCatalogRequested;

        _playerService.OnSongChanged += () => OnPlayerChanged?.Invoke();
        _playerService.OnTimeUpdated += () => OnPlayerChanged?.Invoke();
    }

    /// <summary>
    /// Loads songs from a given folder and initializes the playlist.
    /// </summary>
    /// <param name="folder">Folder containing music files</param>
    public void LoadSongs(string folder)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MyMusicList.Clear();
            foreach (var song in _musicService.GetSongs(folder))
            {
                MyMusicList.Add(song);
            }

            CurrentMusicList.Clear();
            foreach (var song in MyMusicList)
            {
                CurrentMusicList.Add(song);
            }

            _playerService.SetPlaylist(CurrentMusicList.ToList());
            OnPlayerChanged?.Invoke();
        });
    }

    /// <summary>
    /// Handles the reception of a media list from the network.
    /// </summary>
    private void OnMediaListReceived(object? sender, MediaListReceivedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentMusicList.Clear();
            foreach (var media in e.Medias)
            {
                CurrentMusicList.Add(media);
            }

            _playerService.SetPlaylist(CurrentMusicList.ToList());
            OnPlayerChanged?.Invoke();
        });
    }

    /// <summary>
    /// Handles catalog requests from remote media centers.
    /// </summary>
    private async void OnCatalogRequested(object? sender, CatalogRequestEventArgs e)
    {
        var catalog = new Catalog
        {
            MediaCenterId = MyMediaCenter.Id,
            Medias = MyMusicList.ToList()
        };

        var json = JsonSerializer.Serialize(catalog);

        await _agentService.SendEnvelopeAsync(
            new Envelope(
                MyMediaCenter.Id,
                e.RequesterId,
                MessageType.CATALOG,
                json),
            AgentService.BASE_TOPIC);
    }

    /// <summary>
    /// Plays the selected song.
    /// </summary>
    public void Play(MediaDescription song) => _playerService.Play(song);

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause() => _playerService.Pause();

    /// <summary>
    /// Stops playback.
    /// </summary>
    public void Stop() => _playerService.Stop();

    /// <summary>
    /// Plays the next song.
    /// </summary>
    public void PlayNext() => _playerService.PlayNext();

    /// <summary>
    /// Plays the previous song.
    /// </summary>
    public void PlayPrevious() => _playerService.PlayPrevious();

    /// <summary>
    /// Currently playing song.
    /// </summary>
    public MediaDescription? CurrentSong => _playerService.CurrentSong;

    /// <summary>
    /// Indicates whether the player is currently playing.
    /// </summary>
    public bool IsPlaying => _playerService.IsPlaying;

    /// <summary>
    /// Elapsed playback time.
    /// </summary>
    public TimeSpan Elapsed => _playerService.Elapsed;

    /// <summary>
    /// Cleans up event subscriptions.
    /// </summary>
    public void Dispose()
    {
        _agentService.MediaListReceived -= OnMediaListReceived;
        _agentService.CatalogRequested -= OnCatalogRequested;
    }
}
