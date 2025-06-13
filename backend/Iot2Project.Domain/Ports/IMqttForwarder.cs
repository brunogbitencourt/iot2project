using Iot2Project.Domain.Entities;

namespace Iot2Project.Domain.Ports
{
    public interface IMqttForwarder
    {
        Task ForwardAsync(DeviceData data, string kafkaTopic, CancellationToken ct);
    }
}
