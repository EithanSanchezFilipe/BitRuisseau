using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

public class MqttService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public event Action<string, string>? MessageReceived;

    public MqttService(string broker, string username, string password, string clientId)
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
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());
            MessageReceived?.Invoke(topic, payload);
            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync()
    {
        await _client.ConnectAsync(_options);

        await _client.SubscribeAsync(
            new MqttTopicFilterBuilder()
                .WithTopic("#")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build()
        );
    }

    public async Task SendMessageAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(message);
    }

    public async Task DisconnectAsync()
    {
        await _client.DisconnectAsync();
    }
}
