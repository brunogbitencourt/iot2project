using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using Iot2Project.Domain.Interfaces;


namespace Iot2Project.Infrastructure.Messaging.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092"
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync(string topic, string message)
        {
            try
            {
                _logger.LogInformation("[KafkaProducer] Enviando mensagem para o tópico  iot2-tanks-test': {message}", topic, message);
                var deliveryResult = await _producer.ProduceAsync("iot2-tanks-test", new Message<Null, string> { Value = message });

                _logger.LogInformation("[KafkaProducer] Mensagem entregue em {Offset}", deliveryResult.TopicPartitionOffset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KafkaProducer] Erro ao enviar mensagem");
            }
        }
    }
}
