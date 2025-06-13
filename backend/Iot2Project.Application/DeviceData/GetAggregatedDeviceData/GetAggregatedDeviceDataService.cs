// GetAggregatedDeviceDataService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot2Project.Application.Common;
using Iot2Project.Application.DeviceData.GetAggregatedDeviceData;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Enums;
using Iot2Project.Domain.Ports;

namespace Iot2Project.Application.DeviceData.GetAggregatedDeviceData
{
    public class GetAggregatedDeviceDataService
    {
        private readonly IDeviceDataRepository _repo;

        public GetAggregatedDeviceDataService(IDeviceDataRepository repo) => _repo = repo;

        public async Task<GetAggregatedDeviceDataResponse> ExecuteAsync(
            GetAggregatedDeviceDataRequest r,
            CancellationToken ct = default)
        {
            // 1) Busca dados brutos
            var allData = await _repo.GetByPeriodAsync(
                r.DeviceId, r.From, r.To, ct);

            // 2) Agrupa conforme bucket
            Func<DateTime, DateTime> floor = r.Interval switch
            {
                TimeBucket.Minute => dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0),
                TimeBucket.Hour => dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0),
                TimeBucket.Day => dt => new DateTime(dt.Year, dt.Month, dt.Day),
                _ => throw new ArgumentOutOfRangeException()
            };

            var grouped = allData
                .GroupBy(d => floor(d.Timestamp))
                .OrderBy(g => g.Key)
                .Select(g => new AggregatedDeviceData
                {
                    Timestamp = g.Key,
                    AvgValue  = g.Average(d => d.Value),
                    MinValue  = g.Min(d => d.Value),
                    MaxValue  = g.Max(d => d.Value)
                })
                .ToList();

            return new GetAggregatedDeviceDataResponse(grouped);
        }
    }
}
