using GixtApiBackend.Application.UseCases.Advertisements;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GixtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementsController : ControllerBase
    {
        private readonly CreateAdvertisement _createAdvertisement;
        private readonly GetAdvertisement _getAdvertisements;
        private readonly UpdateAdvertisement _updateAdvertisement;
        private readonly DeleteAdvertisement _deleteAdvertisement;

        public AdvertisementsController(
            CreateAdvertisement createAdvertisement,
            GetAdvertisement getAdvertisements,
            UpdateAdvertisement updateAdvertisement,
            DeleteAdvertisement deleteAdvertisement
        )
        {
            _createAdvertisement = createAdvertisement;
            _getAdvertisements = getAdvertisements;
            _updateAdvertisement = updateAdvertisement;
            _deleteAdvertisement = deleteAdvertisement;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] AdvertisementDTO advertisement)
        {
            await _createAdvertisement.Execute(advertisement);
            return Ok(new { message = "Advertisement added successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var advertisements = await _getAdvertisements.Execute();
            return Ok(advertisements);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Advertisement advertisement)
        {
            if (id != advertisement.advertisement_id)
                return BadRequest(new { message = "The ID does not match the request body." });

            await _updateAdvertisement.Execute(advertisement);
            return Ok(new { message = "Advertisement updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteAdvertisement.Execute(id);
            return Ok(new { message = "Advertisement deleted successfully" });
        }
    }

}
