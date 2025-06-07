using System.Text.Json;
using Confluent.Kafka;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Iot2Project.Infrastructure.Messaging.Kafka;

public class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaMessagePublisher> _log;

    public KafkaMessagePublisher(IConfiguration cfg, ILogger<KafkaMessagePublisher> log)
    {
        _log = log;

        var config = new ProducerConfig
        {
            BootstrapServers = cfg["Kafka:BootstrapServers"] ?? "kafka:9092",
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string topic, DeviceData data, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(data);
        var msg = new Message<string, string>
        {
            Key = data.DeviceId.ToString(),
            Value = json
        };

        try
        {
            var result = await _producer.ProduceAsync(topic, msg, ct);
            _log.LogDebug("Kafka ↑ {Topic} ({Offset})", topic, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _log.LogError(ex, "Erro ao publicar mensagem no Kafka (topic={Topic})", topic);
        }
    }

    public ValueTask DisposeAsync()
    {
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}
