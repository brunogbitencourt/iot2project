using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.DeviceData
{
    /// <summary>
    /// Data Transfer Object para os registros de telemetria (device_data).
    /// </summary>
    public record DeviceDataDto(
        int DeviceDataId,
        int DeviceId,
        DateTime Timestamp,
        float Value,
        int UserId,
        string? Command
    );
}
