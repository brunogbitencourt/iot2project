using Iot2Project.Application.Devices.CreateDevice;
using Iot2Project.Application.Devices.DeleteDevice;
using Iot2Project.Application.Devices.GetDevices;
using Iot2Project.Application.Devices.UpdateDevice;
using Iot2Project.Application.Users.CreateUser;
using Iot2Project.Application.Users.GetAllUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Iot2Project.API.Controllers
{
    /// <summary>
    /// Opera um CRUDE (Create, Read, Update, Delete, Exclude) de dispositivos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly GetAllDevicesService _getAll;
        private readonly CreateDeviceService _create;
        private readonly UpdateDeviceService _update;
        private readonly DeleteDeviceService _delete;


        public DevicesController(
            GetAllDevicesService getAll, CreateDeviceService create, UpdateDeviceService update, DeleteDeviceService delete)

        {
            _getAll = getAll;
            _create=create;
            _update=update;
            _delete = delete;
        }

        /// <summary>
        /// Lista todos os dispositivos cadastrados.
        /// </summary>
        // <param name="ct"></param>
        // <returns> </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var devices = await _getAll.ExecuteAsync(ct);
            return Ok(devices);
        }


        /// <summary>
        /// Consulta device pelo id
        /// </summary>
        /// <param name="id">Id do device</param>

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDeviceById(int id, CancellationToken ct)
        {
            var device = await _getAll.ExecuteAsync(ct);
            var foundDevice = device.FirstOrDefault(d => d.DeviceId == id);
            if (foundDevice is null)
            {
                return NotFound();
            }
            return Ok(foundDevice);
        }


        /// <summary>
        ///  Cria um novo dispositivo.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(
        [FromBody] CreateDeviceRequest dto,
        CancellationToken ct)
        {
            var created = await _create.ExecuteAsync(dto, ct);
            return CreatedAtAction(nameof(GetDeviceById), new { id = created.DeviceId }, created);
        }


        /// <summary>
        /// Atualiza campos do dispositvo
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateDeviceDTO dto, CancellationToken ct)
        {
            Console.WriteLine("Vai atualizar");
            var updated = await _update.ExecuteAsync(id, dto, ct);
            return updated is null ? NotFound() : Ok(updated);
        }


        /// <summary>
        /// Deleta um dispositivo logicamente
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            bool ok = await _delete.DeleteDeviceAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
