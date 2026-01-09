using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

namespace BitRuisseau.Services;

/// <summary>
/// Service responsible for MQTT communication.
/// Handles connection, subscription and message publishing.
/// </summary>
public class MqttService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    /// <summary>
    /// Event triggered when a message is received.
    /// </summary>
    public event Action<string, string>? MessageReceived;

    /// <summary>
    /// Initializes a new instance of the MqttService.
    /// </summary>
    /// <param name="broker">MQTT broker address</param>
    /// <param name="username">Broker username</param>
    /// <param name="password">Broker password</param>
    /// <param name="clientId">MQTT client identifier</param>
    public MqttService(
        string broker,
        string username,
        string password,
        string clientId)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .Build();

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(
                e.ApplicationMessage.Payload ?? Array.Empty<byte>());

            MessageReceived?.Invoke(topic, payload);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Connects to the MQTT broker and subscribes to all topics.
    /// </summary>
    public async Task ConnectAsync()
    {
        await _client.ConnectAsync(_options);

        await _client.SubscribeAsync(
            new MqttTopicFilterBuilder()
                .WithTopic("#")
                .WithQualityOfServiceLevel(
                    MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
    }

    /// <summary>
    /// Publishes a message to the given topic.
    /// </summary>
    /// <param name="topic">Target topic</param>
    /// <param name="payload">Message payload</param>
    public async Task SendMessageAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(
                MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(message);
    }

    /// <summary>
    /// Disconnects from the MQTT broker.
    /// </summary>
    public async Task DisconnectAsync()
    {
        await _client.DisconnectAsync();
    }
}
