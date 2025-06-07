using Confluent.Kafka;
using Iot2Project.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Iot2Project.Domain.Ports; // onde está IMessagePublisher

namespace Iot2Project.Infrastructure.Messaging.Mqtt;


public class KafkaMessagePublisher 

{
    private readonly IProducer<string, byte[]> _producer;
    private readonly ILogger<KafkaMessagePublisher> _log;

    public KafkaMessagePublisher(IConfiguration cfg,
                                 ILogger<KafkaMessagePublisher> log)
    {
        _log = log;

        var config = new ProducerConfig
        {
            BootstrapServers = cfg["Kafka:BootstrapServers"] ?? "kafka:9092",
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, byte[]>(config).Build();
    }

    public async Task PublishAsync(string topic, byte[] payload, CancellationToken ct = default)
    {
        var msg = new Message<string, byte[]> { Value = payload };

        await _producer.ProduceAsync(topic, msg, ct);
        _log.LogDebug("Kafka ↑ {Topic} ({Size} B)", topic, payload.Length);
    }

    public ValueTask DisposeAsync()
    {
        _producer.Dispose(); // método síncrono
        return ValueTask.CompletedTask;
    }

}
