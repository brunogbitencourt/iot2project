// GetLatestDeviceDataService.cs
using System.Threading;
using System.Threading.Tasks;
using Iot2Project.Application.Common;
using Iot2Project.Application.DeviceData.GetLatestDeviceData;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;

namespace Iot2Project.Application.DeviceData.GetLatestDeviceData
{
    public class GetLatestDeviceDataService
    {
        private readonly IDeviceDataRepository _repo;

        public GetLatestDeviceDataService(IDeviceDataRepository repo) => _repo = repo;

        public async Task<GetLatestDeviceDataResponse> ExecuteAsync(
            GetLatestDeviceDataRequest r,
            CancellationToken ct = default)
        {
            var data = await _repo.GetLatestAsync(r.DeviceId, ct);

            var dto = data is null
                ? null
                : new DeviceDataDto(
                    data.DeviceDataId,
                    data.DeviceId,
                    data.Timestamp,
                    data.Value,
                    data.UserId,
                    data.Command
                  );

            return new GetLatestDeviceDataResponse(dto);
        }
    }
}