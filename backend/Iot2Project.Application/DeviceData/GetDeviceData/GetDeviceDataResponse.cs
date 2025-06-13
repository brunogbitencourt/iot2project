using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot2Project.Application.DeviceData;

namespace Iot2Project.Application.DeviceData.GetDeviceData
{
    using Iot2Project.Application.Common;
    using Iot2Project.Domain.Entities;
    using Iot2Project.Domain.Ports;

    public record GetDeviceDataResponse(PagedResult<DeviceDataDto> Result);

    // Application/Devices/GetDeviceData/GetDeviceDataService.cs
    public class GetDeviceDataService
    {
        private readonly IDeviceDataRepository _repo;
        public GetDeviceDataService(IDeviceDataRepository repo) => _repo = repo;

        public async Task<GetDeviceDataResponse> ExecuteAsync(GetDeviceDataRequest r, CancellationToken ct)
        {
            // pega tudo do domínio
            var all = await _repo.GetByPeriodAsync(r.DeviceId, r.From, r.To, ct);

            // aplica paginação em Application
            var pageItems = all
              .Skip((r.Page - 1) * r.PageSize)
              .Take(r.PageSize)
              .Select(d => MapToDto(d))
              .ToList();
            var total = all.Count();

            var paged = new PagedResult<DeviceDataDto>
            {
                Items      = pageItems,
                Page       = r.Page,
                PageSize   = r.PageSize,
                TotalItems = total
            };



            return new GetDeviceDataResponse(paged);
        }

        private static DeviceDataDto MapToDto(DeviceData d)
            => new(d.DeviceDataId,  d.DeviceId, d.Timestamp, d.Value, d.UserId, d.Command);
    }
}
