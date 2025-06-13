using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Iot2Project.Application.Devices.GetDevices
{
    public record DeviceDto(
        int DeviceId,
        int UserId,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string ConnectedPort,
        string Name,
        string Type,
        string Category,
        string Unit,
        string MqttTopic,
        string KafkaTopic
    );
}    