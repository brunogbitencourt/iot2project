using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Worker.Services;

public sealed class MqttSubscriberService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttSubscriberService> _logger;

    public MqttSubscriberService(
        IMqttClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttSubscriberService> logger)
    {
        _client       = client;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // 1) Handler de mensagem com log de recebimento
        _client.ApplicationMessageReceivedAsync += async ea =>
        {
            var topic = ea.ApplicationMessage.Topic;
            var raw = ea.ApplicationMessage.PayloadSegment.ToArray();
            _logger.LogInformation("📬 Mensagem recebida no tópico {Topic} ({Length} bytes)", topic, raw.Length);

            // converte payload em texto UTF-8
            var text = Encoding.UTF8.GetString(raw);
            _logger.LogInformation("🔍 Payload como texto: {Text}", text);

            DeviceData incoming;
            try
            {
                incoming = JsonSerializer.Deserialize<DeviceData>(
                    text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                )!;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "❌ Falha ao desserializar payload JSON.");
                return;
            }

            if (incoming is null)
            {
                _logger.LogWarning("❌ Payload JSON resultou em null.");
                return;
            }


            using var scope = _scopeFactory.CreateScope();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var forwarder = scope.ServiceProvider.GetRequiredService<IMqttForwarder>();

            // busca apenas o device para este tópico
            var device = await deviceRepo.GetByMqttTopicAsync(topic);
            if (device == null || string.IsNullOrWhiteSpace(device.KafkaTopic))
            {
                _logger.LogWarning("Tópico MQTT sem mapeamento no banco: {Topic}", topic);
                return;
            }


            await forwarder.ForwardAsync(incoming, device.KafkaTopic, ct);
        };

        // 2) Conecta ao broker
        await _client.ConnectAsync(BuildOptions(), ct);

        // 3) Busca dispositivos e se inscreve, com log de cada inscrição
        using var subScope = _scopeFactory.CreateScope();
        var deviceRepoAll = subScope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var devices = (await deviceRepoAll.GetAllAsync()).ToList();
        var subscribedTopics = new List<string>();

        _logger.LogInformation("🔍 Dispositivos lidos do banco: {Count}", devices.Count);
        foreach (var d in devices)
        {
            _logger.LogInformation("  • DeviceId={DeviceId}, MqttTopic={MqttTopic}, KafkaTopic={KafkaTopic}",
                d.DeviceId, d.MqttTopic, d.KafkaTopic);

            if (string.IsNullOrWhiteSpace(d.MqttTopic))
                continue;

            await _client.SubscribeAsync(d.MqttTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, ct);
            subscribedTopics.Add(d.MqttTopic);
            _logger.LogInformation("✅ Inscrito no tópico MQTT: {Topic}", d.MqttTopic);
        }

        // 4) Log final da lista completa
        if (subscribedTopics.Any())
        {
            _logger.LogInformation("🔍 Tópicos MQTT atualmente inscritos: {Topics}",
                string.Join(", ", subscribedTopics));
        }
        else
        {
            _logger.LogWarning("⚠️ Nenhum tópico MQTT foi inscrito (lista do banco vazia ou com tópicos inválidos).");
        }

        _logger.LogInformation("Assinante MQTT iniciado com tópicos dinâmicos.");
        await Task.Delay(Timeout.Infinite, ct);
    }

    private static MqttClientOptions BuildOptions() =>
        new MqttClientOptionsBuilder()
            .WithTcpServer("test.mosquitto.org", 1883)
            .WithClientId($"worker-{Guid.NewGuid():N}")
            .Build();
}
