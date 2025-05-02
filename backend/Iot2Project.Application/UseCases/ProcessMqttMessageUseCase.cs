// UseCases/ProcessMqttMessageUseCase.cs
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Interfaces;

public class ProcessMqttMessageUseCase
{
    private readonly IKafkaProducer _producer;

    public ProcessMqttMessageUseCase(IKafkaProducer producer)
    {
        _producer = producer;
    }

    public async Task ExecuteAsync(MqttMessage message)
    {
        // Lógica de roteamento ou pré-processamento (opcional)
        await _producer.PublishAsync("iot2-tanks-test", message.Payload);
    }
}
