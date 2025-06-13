using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;
using Microsoft.Extensions.Logging;

public sealed class MqttForwarder : IMqttForwarder
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<MqttForwarder> _logger;

    public MqttForwarder(
        IMessagePublisher publisher,
        ILogger<MqttForwarder> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    // ← Ordem e tipos exatamente como na interface
    public async Task ForwardAsync(DeviceData data, string kafkaTopic, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(kafkaTopic))
        {
            _logger.LogWarning(
                "Tópico Kafka não informado para deviceId={DeviceId}",
                data.DeviceId
            );
            return;
        }

        await _publisher.PublishAsync(kafkaTopic, data, ct);
        _logger.LogInformation(
            "DeviceData (ID={DeviceId}) → Kafka {KafkaTopic}",
            data.DeviceId, kafkaTopic
        );
    }
}
