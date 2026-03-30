
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Locations;
using GixtApiBackend.Application.UseCases.Users;
using Microsoft.AspNetCore.Mvc;


namespace GixtApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly CreateLocation _createLocation;
        private readonly GetLocation _getLocations;
        private readonly UpdateLocation _updateLocation;
        private readonly DeleteLocation _deleteLocation;
        private readonly GetLocationById _getLocationById;
        private readonly GetLocationByUserId _getLocationByUserId;

        public LocationController(
            CreateLocation createLocation,
            GetLocation getLocations,
            UpdateLocation updateLocation,
            DeleteLocation deleteLocation,
            GetLocationById getLocationById,
            GetLocationByUserId getLocationByUserId
        )
        {
            _createLocation = createLocation;
            _getLocations = getLocations;
            _updateLocation = updateLocation;
            _deleteLocation = deleteLocation;
            _getLocationById = getLocationById;
            _getLocationByUserId = getLocationByUserId;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] LocationDTO dto)
        {
            try
            {
                await _createLocation.Execute(dto);
                return Ok(new { message = "Location added successfully" });
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
                var locations = await _getLocations.Execute();
                return Ok(locations);
            }
            catch (Exception ex)when
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUserId(Guid id)
        {
            var location = await _getLocationByUserId.Execute(id);

            if (location == null)
                return NotFound();

            return Ok(location);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var location = await _getLocationById.Execute(id);

            if (location == null)
                return NotFound();

            return Ok(location);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromForm] LocationDTO dto)
        {
            if (id != dto.user_id)
                return BadRequest(new { message = "The user ID does not match the request body." });

            await _updateLocation.Execute(dto);
            return Ok(new { message = "Location updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _deleteLocation.Execute(id);
                return Ok(new { message = "Location deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
           

        }
    }

}
