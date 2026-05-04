
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Services;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace GixtApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly CreateService _createService;
        private readonly GetService _getServices;
        private readonly UpdateService _updateService;
        private readonly DeleteService _deleteService;
        private readonly GetServiceById _getServiceById;
        private readonly GetServiceByIdWorker _getServiceByIdWorker;
        private readonly GetServiceByCategory _getServiceByCategory;
        private readonly GetServiceLocation _getServiceLocation;
        public ServicesController(
            CreateService createService,
            GetService getServices,
            UpdateService updateService,
            DeleteService deleteService,
            GetServiceById getServiceById,
            GetServiceByIdWorker getServiceByIdWorker,
            GetServiceByCategory getServiceByCategory,
            GetServiceLocation getServiceLocation
        )
        {
            _createService = createService;
            _getServices = getServices;
            _updateService = updateService;
            _deleteService = deleteService;
            _getServiceById = getServiceById;
            _getServiceByIdWorker = getServiceByIdWorker;
            _getServiceByCategory = getServiceByCategory;
            _getServiceLocation = getServiceLocation;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ServiceDTO dto)
        {
            await _createService.Execute(dto);
            return Ok(new { message = "Service added successfully" });
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    var services = await _getServices.Execute();
        //    return Ok(services);

        //}

        [HttpGet]
        public async Task<IActionResult> GetLongitude(decimal latitude,decimal longitude,double rangoKm)
        {
            var services = await _getServiceLocation.Execute(latitude,longitude,rangoKm);
            return Ok(services);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id, Guid userId)
        {
            var service = await _getServiceById.Execute(id, userId);

            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [HttpGet("worker/{id}")]
        public async Task<IActionResult> GetByIdWorker(Guid id)
        {
            var service = await _getServiceByIdWorker.Execute(id);

            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetByCategory(int id, int pageNumber = 1)
        {
            
            var service = await _getServiceByCategory.Execute(id, pageNumber);
               return Ok(service);
            
          
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Service service)
        {
            if (id != service.service_id)
                return BadRequest(new { message = "The ID does not match the request body." });

            await _updateService.Execute(service);
            return Ok(new { message = "Service updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteService.Execute(id);
            return Ok(new { message = "Service deleted successfully" });
        }
    }

}
