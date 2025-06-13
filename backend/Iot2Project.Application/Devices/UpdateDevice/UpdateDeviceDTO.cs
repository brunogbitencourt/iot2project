using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Devices.UpdateDevice
{
    public record UpdateDeviceDTO(
        int DeviceId,
        string Name,
        string Type,
        string Category,
        string Unit,
        string ConnectedPort,
        string KafkaTopic,
        string MqttTopic,
        int UserId
    );
}
