using BitRuisseau.Models;
using BitRuisseau.Models.Protocol;
using System.Text.Json;

namespace BitRuisseau.Services
{
    /// <summary>
    /// Service managing MQTT communication between MediaCenters and node management.
    /// </summary>
    public class AgentService
    {
        /// <summary>Base topic used for all MQTT messages.</summary>
        public const string BASE_TOPIC = "powercher/bitruisseau";

        private readonly MqttService _mqtt;
        private readonly Dictionary<string, MediaCenter> _nodes = new();
        private readonly object _lock = new();

        private MediaCenter _mediaCenter;

        /// <summary>Event triggered when the list of known nodes is updated.</summary>
        public event EventHandler<IReadOnlyCollection<MediaCenter>>? NodesUpdated;

        /// <summary>Event triggered when a media catalog is received.</summary>
        public event EventHandler<MediaListReceivedEventArgs>? MediaListReceived;

        /// <summary>Event triggered when a catalog request is received.</summary>
        public event EventHandler<CatalogRequestEventArgs>? CatalogRequested;

        /// <summary>
        /// Constructor for the AgentService.
        /// </summary>
        /// <param name="mqtt">The MQTT service used for communication.</param>
        public AgentService(MqttService mqtt)
        {
            _mqtt = mqtt ?? throw new ArgumentNullException(nameof(mqtt));

            // Subscribe to incoming MQTT messages
            _mqtt.MessageReceived += (topic, payload) =>
            {
                var env = Envelope.FromJson(payload);
                if (env != null)
                {
                    _ = HandleEnvelope(env);
                }
            };
        }

        /// <summary>
        /// Starts the service and connects to the MQTT broker.
        /// </summary>
        /// <param name="mc">The local MediaCenter to register.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task StartAsync(MediaCenter mc)
        {
            _mediaCenter = mc ?? throw new ArgumentNullException(nameof(mc));
            await _mqtt.ConnectAsync();
            await BroadcastPresenceAsync();
        }

        /// <summary>
        /// Stops the service and sends a farewell message.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task StopAsync()
        {
            var goodbye = new Envelope(
                _mediaCenter.Id,
                null,
                MessageType.I_AM_OUT,
                JsonSerializer.Serialize(_mediaCenter)
            );

            await SendEnvelopeAsync(goodbye, BASE_TOPIC);
            await _mqtt.DisconnectAsync();
        }

        /// <summary>
        /// Sends an envelope on a given MQTT topic.
        /// </summary>
        /// <param name="env">Envelope to send.</param>
        /// <param name="topic">Target MQTT topic.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task SendEnvelopeAsync(Envelope env, string topic)
            => _mqtt.SendMessageAsync(topic, env.ToJson());

        /// <summary>
        /// Broadcasts the presence of this MediaCenter on the network.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private Task BroadcastPresenceAsync()
        {
            var hello = new Envelope(
                _mediaCenter.Id,
                null,
                MessageType.WHO_IS_THERE,
                JsonSerializer.Serialize(_mediaCenter)
            );

            return SendEnvelopeAsync(hello, BASE_TOPIC);
        }

        /// <summary>
        /// Handles incoming envelopes from the network.
        /// </summary>
        /// <param name="env">The received envelope.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task HandleEnvelope(Envelope env)
        {
            if (env.SenderId == _mediaCenter.Id)
                return;

            switch (env.Type)
            {
                case MessageType.I_AM_HERE:
                case MessageType.WHO_IS_THERE:
                    var mc = JsonSerializer.Deserialize<MediaCenter>(env.Message);
                    if (mc != null)
                        UpdateNode(mc);

                    if (env.Type == MessageType.WHO_IS_THERE)
                    {
                        await SendEnvelopeAsync(
                            new Envelope(
                                _mediaCenter.Id,
                                env.SenderId,
                                MessageType.I_AM_HERE,
                                JsonSerializer.Serialize(_mediaCenter)
                            ),
                            BASE_TOPIC
                        );
                    }
                    break;

                case MessageType.I_AM_OUT:
                    RemoveNode(env.SenderId);
                    break;

                case MessageType.CATALOG:
                    if (env.ReceiverId != _mediaCenter.Id) break;
                    var catalog = JsonSerializer.Deserialize<Catalog>(env.Message);
                    if (catalog != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                            MediaListReceived?.Invoke(this,
                                new MediaListReceivedEventArgs
                                {
                                    SenderId = env.SenderId,
                                    Medias = catalog.Medias
                                })
                        );
                    }
                    break;

                case MessageType.CATALOG_REQUEST:
                    if (env.ReceiverId != _mediaCenter.Id) break;
                    MainThread.BeginInvokeOnMainThread(() =>
                        CatalogRequested?.Invoke(this,
                            new CatalogRequestEventArgs
                            {
                                RequesterId = env.SenderId
                            })
                    );
                    break;
            }
        }

        /// <summary>
        /// Adds or updates a MediaCenter node.
        /// </summary>
        /// <param name="mc">The MediaCenter to add or update.</param>
        private void UpdateNode(MediaCenter mc)
        {
            lock (_lock)
                _nodes[mc.Id] = mc;

            RaiseNodesUpdated();
        }

        /// <summary>
        /// Removes a MediaCenter node.
        /// </summary>
        /// <param name="id">The ID of the node to remove.</param>
        private void RemoveNode(string id)
        {
            lock (_lock)
                _nodes.Remove(id);

            RaiseNodesUpdated();
        }

        /// <summary>
        /// Raises the NodesUpdated event with a snapshot of current nodes.
        /// </summary>
        private void RaiseNodesUpdated()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IReadOnlyCollection<MediaCenter> snapshot;
                lock (_lock)
                    snapshot = _nodes.Values.ToList();

                NodesUpdated?.Invoke(this, snapshot);
            });
        }
    }
}
