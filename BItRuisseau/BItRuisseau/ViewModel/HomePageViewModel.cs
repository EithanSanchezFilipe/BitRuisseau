using BitRuisseau.Models;
using BitRuisseau.Protocol;
using BitRuisseau.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BitRuisseau.ViewModel;

public class HomePageViewModel : IDisposable
{
    private readonly AgentService _agent;
    private readonly MusicService _musicService;
    private readonly PlayerService _player;

    public MediaCenter MyMediaCenter { get; }
    public ObservableCollection<MediaDescription> MyMusicList { get; } = new();
    public ObservableCollection<MediaDescription> CurrentMusicList { get; } = new();

    public event Action? OnPlayerChanged;

    public HomePageViewModel(AgentService agent, MusicService musicService, PlayerService player, LocalMediaCenterService localMediaCenter)
    {
        _agent = agent;
        _musicService = musicService;
        _player = player;

        MyMediaCenter = localMediaCenter.Instance;

        _agent.MediaListReceived += OnMediaListReceived;
        _agent.CatalogRequested += OnCatalogRequested;

        _player.OnSongChanged += () => OnPlayerChanged?.Invoke();
        _player.OnTimeUpdated += () => OnPlayerChanged?.Invoke();
    }

    public void LoadSongs(string folder)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MyMusicList.Clear();
            foreach (var song in _musicService.GetSongs(folder))
                MyMusicList.Add(song);

            CurrentMusicList.Clear();
            foreach (var song in MyMusicList)
                CurrentMusicList.Add(song);

            _player.SetPlaylist(CurrentMusicList.ToList());

            OnPlayerChanged?.Invoke();
        });
    }

    private void OnMediaListReceived(object? sender, MediaListReceivedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentMusicList.Clear();
            foreach (var music in e.Medias)
                CurrentMusicList.Add(music);

            _player.SetPlaylist(CurrentMusicList.ToList());
            OnPlayerChanged?.Invoke();
        });
    }

    private async void OnCatalogRequested(object? sender, CatalogRequestEventArgs e)
    {
        var catalog = new Catalog
        {
            MediaCenterId = MyMediaCenter.Id,
            Medias = MyMusicList.ToList()
        };

        var json = JsonSerializer.Serialize(catalog);

        await _agent.SendEnvelopeAsync(
            new Envelope(MyMediaCenter.Id, e.RequesterId, MessageType.CATALOG, json),
            AgentService.BASE_TOPIC);
    }

    public void Play(MediaDescription song) => _player.Play(song);
    public void Pause() => _player.Pause();
    public void Stop() => _player.Stop();
    public void PlayNext() => _player.PlayNext();
    public void PlayPrevious() => _player.PlayPrevious();

    public MediaDescription? CurrentSong => _player.CurrentSong;
    public bool IsPlaying => _player.IsPlaying;
    public TimeSpan Elapsed => _player.Elapsed;

    public void Dispose()
    {
        _agent.MediaListReceived -= OnMediaListReceived;
        _agent.CatalogRequested -= OnCatalogRequested;
    }
}
