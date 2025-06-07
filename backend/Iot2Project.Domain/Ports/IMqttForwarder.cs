using Iot2Project.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Domain.Ports;

public interface IMqttForwarder
{
    Task ForwardAsync(DeviceData data, CancellationToken ct = default, string kafkaTopic = "");
}
