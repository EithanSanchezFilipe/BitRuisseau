using BitRuisseau.Models;
using BitRuisseau.Protocol;
using System.Text.Json;

namespace BitRuisseau.Services
{
    public class AgentService
    {
        public const string BASE_TOPIC = "powercher/bitruisseau";
        private readonly MqttService _mqtt;
        private readonly MusicService _musicService;
        const int FRAGMENT_SIZE = 32 * 1024;

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
                try {
                    Envelope? envelope = Envelope.FromJson(payload);
                    if (envelope != null)
                    {
                        HandleEnvelope(envelope);
                        OnEnvelopeReceived?.Invoke(envelope);
                    }
                
                }
                catch (Exception ex)
                {
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
            var goodbye = new Envelope(_mediaCenter.Id, null, MessageType.I_AM_OUT, JsonSerializer.Serialize(_mediaCenter));
            await SendEnvelopeAsync(goodbye, BASE_TOPIC);
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

            await SendEnvelopeAsync(hello, BASE_TOPIC);
        }

        private async Task HandleEnvelope(Envelope envelope)
        {
            if (envelope.SenderId == _mediaCenter.Id)
                return;

            switch (envelope.Type)
            {
                case MessageType.I_AM_HERE:
                    {

                        MediaCenter mediaCenter =
                            JsonSerializer.Deserialize<MediaCenter>(envelope.Message)!;

                        //n'est pas idempotant avec tout le monde aucune idée pourquoi
                        if (!_nodes.Any(n => n.Id == mediaCenter.Id))
                        {
                            _nodes.Add(mediaCenter);
                            OnNodesUpdated?.Invoke();
                        }
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
                            sender.Id,
                            MessageType.I_AM_HERE,
                            JsonSerializer.Serialize(_mediaCenter)
                        ),
                        BASE_TOPIC
                    );
                    break;
                case MessageType.I_AM_OUT:
                    _nodes.Remove(_nodes.Where(node => node.Id == envelope.SenderId).FirstOrDefault());
                    OnNodesUpdated?.Invoke();
                    break;

                case MessageType.CATALOG:
                    if (envelope.ReceiverId != _mediaCenter.Id)
                        break;
                    
                    Catalog? catalog =
                        JsonSerializer.Deserialize<Catalog>(envelope.Message);

                    if (catalog == null)
                        break;

                    var musics = catalog.Medias;

                    _musicService.CurrentMusicList = musics;
                    OnMediaListReceived?.Invoke();
                    break;

                case MessageType.CATALOG_REQUEST:
                    if (envelope.ReceiverId == _mediaCenter.Id)
                    {
                        Catalog medias = new Catalog() { MediaCenterId = _mediaCenter.Id, Medias= _musicService.MyMusicList };
                        // send back my music
                        string json = JsonSerializer.Serialize<Catalog>(medias);

                        await SendEnvelopeAsync(new Envelope(_mediaCenter.Id, envelope.SenderId, MessageType.CATALOG, json), $"{BASE_TOPIC}");
                    }
                    break;
                case MessageType.FRAGMENT_REQUEST:
                    if(envelope.ReceiverId != _mediaCenter.Id)
                        break;
                    Fragment request = JsonSerializer.Deserialize<Fragment>(envelope.Message)!;


                    MediaDescription? media = _musicService.MyMusicList
                        .FirstOrDefault(m => m.Id == request.MediaId);

                    if (media?.FilePath == null)
                        break;

                    //checks if the start index is lower than 0 if that the case we start at the beginning of the file
                    long start = request.StartIndex < 0 ? 0 : request.StartIndex;

                    //checks the last byte of the file but if the fragment wanted is bigger than the song it takes the last song byte as end index
                    long end = Math.Min(start + FRAGMENT_SIZE - 1, media.Size - 1);

                    //computes the number of bytes needed to 1 fragment
                    int length = (int)(end - start + 1);

                    byte[] buffer = new byte[length];
                    using (var fs = new FileStream(media.FilePath, FileMode.Open, FileAccess.Read))
                    {
                        //puts the read index to the start index
                        fs.Seek(start, SeekOrigin.Begin);
                        int read = fs.Read(buffer, 0, length);
                        if (read != length)
                        {
                            Array.Resize(ref buffer, read); //resizes the array to not send inutile bytes
                            end = start + read - 1;
                        }
                    }

                    Fragment fragment = new Fragment
                    {
                        MediaId = media.Id,
                        StartIndex = request.StartIndex,
                        EndIndex = request.EndIndex,
                        Content = Convert.ToBase64String(buffer)
                    };

                    await SendEnvelopeAsync(
                        new Envelope(
                            _mediaCenter.Id,
                            envelope.SenderId,
                            MessageType.FRAGMENT,
                            JsonSerializer.Serialize(fragment)
                        ),
                        $"{BASE_TOPIC}/{media.Id}"
                    );

                    break;

            }
        }
    }
}
