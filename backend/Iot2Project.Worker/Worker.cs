using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Iot2Project.Domain.Entities;
using Iot2Project.Infrastructure.Messaging.Mqtt;

public class Worker : BackgroundService
{
    private readonly MqttMessageForwarder _forwarder;
    private readonly ILogger<Worker> _logger;
    private IMqttClient _mqttClient;

    public Worker(MqttMessageForwarder forwarder, ILogger<Worker> logger)
    {
        _forwarder = forwarder;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("test.mosquitto.org", 1883) // ou nome do serviço docker, ex: mqtt_broker
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());

            _logger.LogInformation("Mensagem recebida no tópico '{topic}': {message}", topic, message);

            var mqttMessage = new MqttMessage
            {
                Topic = topic,
                Payload = message
            };

            // Encaminha usando o forwarder
            await _forwarder.ForwardAsync(mqttMessage);
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
