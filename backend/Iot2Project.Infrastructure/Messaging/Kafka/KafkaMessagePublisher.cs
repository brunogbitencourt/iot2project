using Confluent.Kafka;
using Iot2Project.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Confluent.Kafka.ConfigPropertyNames;

public sealed class KafkaMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IProducer<string, byte[]> _kafkaProducer;          // ← nome exclusivo
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(
        IConfiguration cfg,
        ILogger<KafkaMessagePublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = cfg["Kafka:BootstrapServers"] ?? "kafka:9092",
            Acks = Acks.All
        };

        _kafkaProducer = new ProducerBuilder<string, byte[]>(config).Build();
    }

    public async Task PublishAsync(string topic, byte[] payload, CancellationToken ct = default)
    {
        var msg = new Message<string, byte[]> { Value = payload };
        await _kafkaProducer.ProduceAsync(topic, msg, ct);
        _logger.LogDebug("Kafka ↑ {Topic} ({Size} B)", topic, payload.Length);
    }

    public ValueTask DisposeAsync()
    {
        _kafkaProducer.Dispose(); // método síncrono
        return ValueTask.CompletedTask;
    }

}
