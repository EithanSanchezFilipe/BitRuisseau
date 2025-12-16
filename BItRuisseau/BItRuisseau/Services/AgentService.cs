using BitRuisseau.Models;
using BitRuisseau.Protocol;
using System.Text.Json;

namespace BitRuisseau.Services
{
    public class AgentService
    {
        private readonly MqttService _mqtt;
        private readonly MusicService _musicService;


        private List<MediaCenter> _nodes = new();
        public List<MediaCenter> Nodes => _nodes;

        public event Action? OnNodesUpdated;
        public event Action<Envelope>? OnEnvelopeReceived;
        public event Action? OnMediaListReceived;

        private MediaCenter _mediaCenter;

        public AgentService(MqttService mqttService, MusicService musicService)
        {
            _mqtt = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
            _musicService = musicService ?? throw new ArgumentNullException(nameof(_musicService));


            _mqtt.MessageReceived += (topic, payload) =>
            {
                Envelope? envelope = Envelope.FromJson(payload);
                if (envelope != null)
                {
                    HandleEnvelope(envelope);
                    OnEnvelopeReceived?.Invoke(envelope);
                }
            };
        }

        public async Task StartAsync(MediaCenter mediaCenter)
        {
            _mediaCenter = mediaCenter;
            await _mqtt.ConnectAsync();
            await BroadcastPresenceAsync();
        }

        public async Task StopAsync()
        {
            var goodbye = new Envelope(_mediaCenter.Id, null, MessageType.I_AM_OUT, "");
            await SendEnvelopeAsync(goodbye, "users");
            await _mqtt.DisconnectAsync();
        }

        public async Task SendEnvelopeAsync(Envelope envelope, string topic)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            await _mqtt.SendMessageAsync(topic, envelope.ToJson());
        }

        private async Task BroadcastPresenceAsync()
        {
            var hello = new Envelope(
                _mediaCenter.Id,
                null,
                MessageType.WHO_IS_THERE,
                JsonSerializer.Serialize(_mediaCenter)
            );

            await SendEnvelopeAsync(hello, "users");
        }

        private async Task HandleEnvelope(Envelope envelope)
        {
            if (envelope.SenderId == _mediaCenter.Id)
                return;

            switch (envelope.Type)
            {
                case MessageType.I_AM_HERE:
                    try
                    {
                        MediaCenter? mediaCenter =
                            JsonSerializer.Deserialize<MediaCenter>(envelope.Message);

                        if (mediaCenter != null && !_nodes.Any(n => n.Name == mediaCenter.Name))
                        {
                            _nodes.Add(mediaCenter);
                            OnNodesUpdated?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse envelope payload: {ex.Message}");
                    }
                    break;

                case MessageType.WHO_IS_THERE:
                    MediaCenter sender =
                        JsonSerializer.Deserialize<MediaCenter>(envelope.Message);

                    _nodes.Add(sender);
                    OnNodesUpdated?.Invoke();

                    await SendEnvelopeAsync(
                        new Envelope(
                            _mediaCenter.Id,
                            null,
                            MessageType.I_AM_HERE,
                            JsonSerializer.Serialize(_mediaCenter)
                        ),
                        "users"
                    );
                    break;

                case MessageType.CATALOG:
                    if (envelope.ReceiverId != _mediaCenter.Id)
                        break;

                    List<MediaDescription>? descriptions =
                        JsonSerializer.Deserialize<List<MediaDescription>>(envelope.Message);

                    if (descriptions == null)
                        break;

                    var musics = descriptions;

                    _musicService.CurrentMusicList = musics;
                    OnMediaListReceived?.Invoke();
                    break;

                case MessageType.CATALOG_REQUEST:
                    // am i the receiver ?
                    if (envelope.ReceiverId == _mediaCenter.Id)
                    {
                        // send back my music
                        string json = JsonSerializer.Serialize(_musicService.MyMusicList);

                        await SendEnvelopeAsync(new Envelope(_mediaCenter.Id, envelope.SenderId, MessageType.CATALOG, json), "users");
                    }
                    break;


            }
        }
    }
}
