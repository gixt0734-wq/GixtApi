
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Workers;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace GixtApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkersController : ControllerBase
    {
        private readonly CreateWorker _createWorker;
        private readonly CreateInfoWorker _createInfoWorker;
        private readonly GetWorkers _getWorkers;
        private readonly UpdateWorker _updateWorker;
        private readonly UpdateInfoWorker _updateInfoWorker;
        private readonly DeleteWorker _deleteWorker;
        private readonly GetWorkerById _getWorkerById;
        private readonly GetInfoWorkerById _getInfoWorkerById;

        public WorkersController(
            CreateWorker createWorker,
            CreateInfoWorker createInfoWorker,
            GetWorkers getWorkers,
            GetInfoWorkerById getInfoWorkerById,
            UpdateWorker updateWorker,
            DeleteWorker deleteWorker,
            GetWorkerById getWorkerById,
            UpdateInfoWorker updateInfoWorker

        )
        {
            _createWorker = createWorker;
            _createInfoWorker = createInfoWorker;
            _getWorkers = getWorkers;
            _getInfoWorkerById = getInfoWorkerById;
            _updateWorker = updateWorker;
            _deleteWorker = deleteWorker;
            _getWorkerById = getWorkerById;
            _updateInfoWorker = updateInfoWorker;

        }

        [HttpPost]
        public async Task<IActionResult> PostWorker([FromForm] UserDTO dto)
        {
            try
            {
                await _createWorker.Execute(dto);
                return Ok(new { message = "Worker added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("info")]
        public async Task<IActionResult> PostInfoWorker([FromForm] WorkerDTO dto)
        {
            try
            {
                await _createInfoWorker.Execute(dto);
                return Ok(new { message = "Worker added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


       

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var users = await _getWorkers.Execute();
                return Ok(users);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _getWorkerById.Execute(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("info/{id}")]
        public async Task<IActionResult> GetInfoById(Guid id)
        {
            var user = await _getInfoWorkerById.Execute(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromForm] UserUpdateDTO dto)
        {
            if (id != dto.user_id)
                return BadRequest(new { message = "The user ID does not match the request body." });

            await _updateWorker.Execute(dto);
            return Ok(new { message = "Worker updated successfully" });
        }

        [HttpPut("info/{id}")]
        public async Task<IActionResult> PutInfo(Guid id, [FromForm] WorkerDTO dto)
        {
            if (id != dto.user_id)
                return BadRequest(new { message = "The user ID does not match the request body." });

            await _updateInfoWorker.Execute(dto);
            return Ok(new { message = "Worker updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteWorker.Execute(id);
            return Ok(new { message = "Worker deleted successfully" });
        }
    }

}
