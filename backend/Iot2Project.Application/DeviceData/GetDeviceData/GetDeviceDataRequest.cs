using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.DeviceData.GetDeviceData
{
    public record GetDeviceDataRequest(
        int DeviceId,
        DateTime From,
        DateTime To,
        int Page = 1,
        int PageSize = 50
    );

}
