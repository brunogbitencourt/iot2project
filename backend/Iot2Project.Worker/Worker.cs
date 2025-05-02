using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Iot2Project.Domain.Interfaces;
using Iot2Project.Infrastructure.Kafka;

public class Worker : BackgroundService
{
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<Worker> _logger;
    private IMqttClient _mqttClient;

    public Worker(IKafkaProducer kafkaProducer, ILogger<Worker> logger)
    {
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("test.mosquitto.org", 1883) // se estiver em container, use o nome do serviço
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());
            _logger.LogInformation("Mensagem recebida no tópico '{topic}': {message}", topic, message);

            // Publica no Kafka
            await _kafkaProducer.PublishAsync(topic, message);
        };

        _mqttClient.ConnectedAsync += async e =>
        {
            _logger.LogInformation("Conectado ao broker MQTT");
            await _mqttClient.SubscribeAsync("iot2/tanks/test");
        };

        try
        {
            await _mqttClient.ConnectAsync(options, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao MQTT");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
