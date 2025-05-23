using System.Text.Json;
using Confluent.Kafka;
using Iot2Project.Application.Interfaces;
using Iot2Project.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            _logger = logger;
            _scopeFactory = scopeFactory;

            _config = new ConsumerConfig
            {
                BootstrapServers = "kafka:9092", // Se estiver fora do container, use "localhost:9092"
                GroupId = "iot2-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer iniciado para o tópico 'device-data'.");

            try
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
                //consumer.Subscribe("device-data");
                consumer.Subscribe("iot2-tanks-test"); // <- agora igual ao producer


                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        _logger.LogInformation("Mensagem Kafka recebida: {0}", result.Message.Value);

                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IDeviceDataRepository>();

                        var data = JsonSerializer.Deserialize<DeviceData>(
                                     result.Message.Value,
                                        new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                        if (data != null)
                        {

                            _logger.LogInformation("Preparando para salvar: deviceId={0}, value={1}, timestamp={2}",
                                data?.DeviceId, data?.Value, data?.Timestamp);
                            await repo.SaveAsync(data);
                            _logger.LogInformation("Mensagem persistida com sucesso.");
                        }
                        else
                        {
                            _logger.LogWarning("Mensagem Kafka inválida ou nula.");
                        }
                    }
                    catch (ConsumeException ce)
                    {
                        _logger.LogError(ce, "Erro ao consumir mensagem do Kafka.");
                        await Task.Delay(3000, stoppingToken);
                    }
                    catch (JsonException je)
                    {
                        _logger.LogError(je, "Erro ao desserializar mensagem Kafka.");
                        await Task.Delay(3000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro inesperado no processamento da mensagem.");
                        await Task.Delay(3000, stoppingToken);
                    }
                }

                consumer.Close();
            }
            catch (Exception fatal)
            {
                _logger.LogCritical(fatal, "Erro fatal ao iniciar KafkaDeviceDataConsumer.");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
