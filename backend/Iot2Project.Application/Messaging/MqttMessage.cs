namespace Iot2Project.Application.Messaging;

/// <summary>
/// Representa uma mensagem que chegou do broker MQTT.
/// </summary>
public sealed record MqttMessage(string Topic, byte[] Payload);
