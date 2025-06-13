using System.Collections.Generic;
using Iot2Project.Application.Common;

namespace Iot2Project.Application.DeviceData.GetAggregatedDeviceData
{
    public record GetAggregatedDeviceDataResponse(
        IReadOnlyList<AggregatedDeviceData> Items
    );
}
