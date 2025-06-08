using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Iot2Project.Domain.Ports;
using Iot2Project.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot2Project.Application.Interfaces;

namespace Iot2Project.Worker.Kafka
{
    public class KafkaDeviceDataConsumer : BackgroundService
    {
        private readonly ILogger<KafkaDeviceDataConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConsumerConfig _config;

        public KafkaDeviceDataConsumer(
            ILogger<KafkaDeviceDataConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger       = logger;
            _scopeFactory = scopeFactory;
            _config       = new ConsumerConfig
            {
                BootstrapServers  = "kafka:9092",
                GroupId           = "iot2-group",
                AutoOffsetReset   = AutoOffsetReset.Earliest,
                EnableAutoCommit  = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer iniciando...");

            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            using var admin = new AdminClientBuilder(_config).Build();

            // 1) Busca todos os kafka_topic no banco
            using (var scope = _scopeFactory.CreateScope())
            {
                var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
                var devices = await deviceRepo.GetAllAsync();
                var topics = devices
                    .Select(d => d.KafkaTopic)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct()
                    .ToList();

                if (topics.Count==0)
                {
                    _logger.LogWarning("Nenhum kafka_topic encontrado no banco. Consumer não irá se inscrever em nada.");
                }
                else
                {
                    // 2) Cria todos os tópicos via Admin API (se já existirem, apenas captura o warning)
                    try
                    {
                        var specs = topics
                            .Select(t => new TopicSpecification { Name = t, NumPartitions = 1, ReplicationFactor = 1 })
                            .ToList();

                        await admin.CreateTopicsAsync(specs);
                        _logger.LogInformation("Tópicos Kafka garantidos/criados: {Topics}", string.Join(", ", topics));
                    }
                    catch (CreateTopicsException cte)
                    {
                        _logger.LogWarning("Alguns tópicos não puderam ser criados: {Error}",
                            string.Join(", ", cte.Results.Select(r => r.Error.Reason)));
                    }

                    // 3) Inscreve o consumer em todos
                    consumer.Subscribe(topics);
                    _logger.LogInformation("Consumer inscrito em tópicos: {Topics}", string.Join(", ", topics));
                }
            }

            _logger.LogInformation("Kafka consumer iniciado com assinaturas dinâmicas.");

            // 4) Loop de consumo
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    _logger.LogInformation("Mensagem Kafka recebida (tópico={Topic}): {Value}", result.Topic, result.Message.Value);

                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IDeviceDataRepository>();

                    var data = JsonSerializer.Deserialize<DeviceData>(
                        result.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data != null)
                    {
                        _logger.LogInformation(
                            "Persistindo: deviceId={DeviceId}, value={Value}, timestamp={Timestamp}",
                            data.DeviceId, data.Value, data.Timestamp);

                        await repo.SaveAsync(data);
                        _logger.LogInformation("Mensagem persistida com sucesso.");
                    }
                    else
                    {
                        _logger.LogWarning("Payload Kafka inválido ou nulo.");
                    }
                }
                catch (ConsumeException ce)
                {
                    _logger.LogError(ce, "Erro ao consumir mensagem.");
                    await Task.Delay(3000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Cancelamento solicitado. Encerrando consumer Kafka.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado no consumer Kafka.");
                    await Task.Delay(3000, stoppingToken);
                }
            }

            consumer.Close();
            _logger.LogInformation("Consumer Kafka finalizado.");
        }
    }
}
