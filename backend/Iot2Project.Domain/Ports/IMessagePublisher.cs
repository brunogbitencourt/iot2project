namespace Iot2Project.Domain.Ports;

/// <summary>
/// Porta de saída que publica um payload binário em um tópico.
/// O domínio não conhece detalhes do broker.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(string topic,
                      byte[] payload,
                      CancellationToken ct = default);
}
