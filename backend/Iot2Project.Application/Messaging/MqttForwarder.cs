using Iot2Project.Domain.Ports;            // IMessagePublisher
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iot2Project.Application.Configuration;


namespace Iot2Project.Application.Messaging;

public sealed class MqttForwarder : IMqttForwarder
{
    private readonly IMessagePublisher _publisher;
    private readonly IReadOnlyDictionary<string, string> _routeMap;
    private readonly ILogger<MqttForwarder> _logger;

    public MqttForwarder(
        IMessagePublisher publisher,
        IOptions<TopicRoutingOptions> options,
        ILogger<MqttForwarder> logger)
    {
        _publisher = publisher;
        _routeMap  = options.Value;     // dicionário vindo do appsettings
        _logger    = logger;
    }

    public async Task ForwardAsync(MqttMessage message, CancellationToken ct = default)
    {
        if (!_routeMap.TryGetValue(message.Topic, out var kafkaTopic))
        {
            _logger.LogWarning("Tópico MQTT sem mapeamento: {Topic}", message.Topic);
            return;                     // ou lançar exceção, se preferir
        }

        await _publisher.PublishAsync(kafkaTopic, message.Payload, ct);
        _logger.LogInformation("MQTT {Mqtt} → Kafka {Kafka}", message.Topic, kafkaTopic);
    }
}
