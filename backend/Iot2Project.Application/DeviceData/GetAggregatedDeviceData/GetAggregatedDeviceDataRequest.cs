using System;
using Iot2Project.Domain.Enums;

namespace Iot2Project.Application.DeviceData.GetAggregatedDeviceData
{
    public record GetAggregatedDeviceDataRequest(
        int DeviceId,
        DateTime From,
        DateTime To,
        TimeBucket Interval
    );
}
