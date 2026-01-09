using BitRuisseau.Models;
using BitRuisseau.Protocol;
using System.Text.Json;

namespace BitRuisseau.Services;

public class AgentService
{
    public const string BASE_TOPIC = "powercher/bitruisseau";
    private readonly MqttService _mqtt;

    private readonly Dictionary<string, MediaCenter> _nodes = new();
    private readonly object _lock = new();

    public event EventHandler<IReadOnlyCollection<MediaCenter>>? NodesUpdated;
    public event EventHandler<MediaListReceivedEventArgs>? MediaListReceived;
    public event EventHandler<CatalogRequestEventArgs>? CatalogRequested;

    private MediaCenter _mediaCenter;

    public AgentService(MqttService mqtt)
    {
        _mqtt = mqtt;

        _mqtt.MessageReceived += (topic, payload) =>
        {
            var env = Envelope.FromJson(payload);
            if (env != null)
                _ = HandleEnvelope(env);
        };
    }

    public async Task StartAsync(MediaCenter mc)
    {
        _mediaCenter = mc;
        await _mqtt.ConnectAsync();
        await BroadcastPresenceAsync();
    }

    public async Task StopAsync()
    {
        var goodbye = new Envelope(_mediaCenter.Id, null, MessageType.I_AM_OUT,
            JsonSerializer.Serialize(_mediaCenter));
        await SendEnvelopeAsync(goodbye, BASE_TOPIC);
        await _mqtt.DisconnectAsync();
    }

    public Task SendEnvelopeAsync(Envelope env, string topic)
        => _mqtt.SendMessageAsync(topic, env.ToJson());

    private Task BroadcastPresenceAsync()
    {
        var hello = new Envelope(_mediaCenter.Id, null, MessageType.WHO_IS_THERE,
            JsonSerializer.Serialize(_mediaCenter));
        return SendEnvelopeAsync(hello, BASE_TOPIC);
    }

    private async Task HandleEnvelope(Envelope env)
    {
        if (env.SenderId == _mediaCenter.Id)
            return;

        switch (env.Type)
        {
            case MessageType.I_AM_HERE:
            case MessageType.WHO_IS_THERE:
                var mc = JsonSerializer.Deserialize<MediaCenter>(env.Message);
                UpdateNode(mc);

                if (env.Type == MessageType.WHO_IS_THERE)
                    await SendEnvelopeAsync(
                        new Envelope(_mediaCenter.Id, env.SenderId,
                            MessageType.I_AM_HERE, JsonSerializer.Serialize(_mediaCenter)),
                        BASE_TOPIC);
                break;

            case MessageType.I_AM_OUT:
                RemoveNode(env.SenderId);
                break;

            case MessageType.CATALOG:
                if (env.ReceiverId != _mediaCenter.Id) break;
                var cat = JsonSerializer.Deserialize<Catalog>(env.Message);
                MainThread.BeginInvokeOnMainThread(() =>
                    MediaListReceived?.Invoke(this,
                        new MediaListReceivedEventArgs { SenderId = env.SenderId, Medias = cat.Medias }));
                break;

            case MessageType.CATALOG_REQUEST:
                if (env.ReceiverId != _mediaCenter.Id) break;
                MainThread.BeginInvokeOnMainThread(() =>
                    CatalogRequested?.Invoke(this,
                        new CatalogRequestEventArgs { RequesterId = env.SenderId }));
                break;
        }
    }

    private void UpdateNode(MediaCenter mc)
    {
        lock (_lock)
            _nodes[mc.Id] = mc;
        RaiseNodesUpdated();
    }

    private void RemoveNode(string id)
    {
        lock (_lock)
            _nodes.Remove(id);
        RaiseNodesUpdated();
    }

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
