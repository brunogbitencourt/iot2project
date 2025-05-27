using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Application.Messaging;

public interface IMqttForwarder
{
    Task ForwardAsync(MqttMessage message, CancellationToken ct = default);
}
