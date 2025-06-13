using System;
using System.Threading;
using System.Threading.Tasks;
using Iot2Project.Application.Common;
using Iot2Project.Application.DeviceData.GetAggregatedDeviceData;
using Iot2Project.Application.DeviceData.GetDeviceData;
using Iot2Project.Application.DeviceData.GetLatestDeviceData;
using Microsoft.AspNetCore.Mvc;
using Iot2Project.Domain.Enums;
using Iot2Project.Application.DeviceData;

namespace Iot2Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceDataController : ControllerBase
    {
        private readonly GetDeviceDataService _getService;
        private readonly GetAggregatedDeviceDataService _aggService;
        private readonly GetLatestDeviceDataService _latestService;

        public DeviceDataController(
            GetDeviceDataService getService,
            GetAggregatedDeviceDataService aggService,
            GetLatestDeviceDataService latestService)
        {
            _getService    = getService;
            _aggService    = aggService;
            _latestService = latestService;
        }

        /// <summary>
        /// Consulta dados brutos de telemetria paginados.
        /// GET /api/device-data?deviceId=1&from=2025-05-01&to=2025-05-31&page=1&pageSize=50
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DeviceDataDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int deviceId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            if (from > to)
                return BadRequest("'from' deve ser anterior ou igual a 'to'.");

            var req = new GetDeviceDataRequest(deviceId, from, to, page, pageSize);
            var resp = await _getService.ExecuteAsync(req, ct);
            return Ok(resp.Result);
        }

        /// <summary>
        /// Consulta dados agregados (avg/min/max) por bucket de tempo.
        /// GET /api/device-data/aggregate?deviceId=1&from=2025-05-01&to=2025-05-31&interval=Hour
        /// </summary>
        [HttpGet("aggregate")]
        [ProducesResponseType(typeof(IEnumerable<AggregatedDeviceData>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAggregate(
            [FromQuery] int deviceId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string interval,
            CancellationToken ct = default)
        {
            if (from > to)
                return BadRequest("'from' deve ser anterior ou igual a 'to'.");

            if (!Enum.TryParse<TimeBucket>(interval, true, out var bucket))
                return BadRequest("Intervalo inválido. Use Minute, Hour ou Day.");

            var req = new GetAggregatedDeviceDataRequest(deviceId, from, to, bucket);
            var resp = await _aggService.ExecuteAsync(req, ct);
            return Ok(resp.Items);
        }

        /// <summary>
        /// Consulta a última leitura de um dispositivo.
        /// GET /api/device-data/latest?deviceId=1
        /// </summary>
        [HttpGet("latest")]
        [ProducesResponseType(typeof(DeviceDataDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLatest(
            [FromQuery] int deviceId,
            CancellationToken ct = default)
        {
            var resp = await _latestService.ExecuteAsync(
                new GetLatestDeviceDataRequest(deviceId), ct);

            if (resp.Data is null)
                return NotFound();

            return Ok(resp.Data);
        }
    }
}
