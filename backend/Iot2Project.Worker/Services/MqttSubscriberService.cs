using Iot2Project.Application.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Worker.Services;

public sealed class MqttSubscriberService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly IMqttForwarder _forwarder;
    private readonly ILogger<MqttSubscriberService> _logger;

    public MqttSubscriberService(
        IMqttClient client,
        IMqttForwarder forwarder,
        ILogger<MqttSubscriberService> logger)
    {
        _client     = client;
        _forwarder  = forwarder;
        _logger     = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _client.ApplicationMessageReceivedAsync += async ea =>
        {
            var msg = new MqttMessage(
                ea.ApplicationMessage.Topic,
                ea.ApplicationMessage.PayloadSegment.ToArray());

            await _forwarder.ForwardAsync(msg, ct);
        };

        await _client.ConnectAsync(BuildOptions(), ct);
        await _client.SubscribeAsync("iot2/tanks/#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, ct);

        _logger.LogInformation("Assinante MQTT ativo.");
        await Task.Delay(Timeout.Infinite, ct);   // mantém o serviço rodando
    }

    private static MqttClientOptions BuildOptions()
    {
        return new MqttClientOptionsBuilder()
            .WithTcpServer("test.mosquitto.org", 1883) // Broker público
            .WithClientId($"worker-{Guid.NewGuid():N}") // ClientId único
            .Build();
    }

}
