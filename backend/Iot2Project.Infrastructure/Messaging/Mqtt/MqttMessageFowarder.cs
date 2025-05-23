// Infrastructure/Messaging/Mqtt/MqttMessageForwarder.cs
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Interfaces;

namespace Iot2Project.Infrastructure.Messaging.Mqtt;

public class MqttMessageForwarder
{
    private readonly IKafkaProducer _producer;

    public MqttMessageForwarder(IKafkaProducer producer)
    {
        _producer = producer;
    }

    public async Task ForwardAsync(MqttMessage message)
    {
        await _producer.PublishAsync("iot2-tanks-test", message.Payload);
    }
}
