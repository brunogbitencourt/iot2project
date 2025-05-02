namespace Iot2Project.Domain.Entities;

// Entities/MqttMessage.cs
public class MqttMessage
{
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
