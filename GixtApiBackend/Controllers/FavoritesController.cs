using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.UseCases.Favorites;
using GixtApiBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GixtApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly CreateFavorite _createFavorite;
        private readonly GetFavorite _getFavorites;
        private readonly UpdateFavorite _updateFavorite;
        private readonly DeleteFavorite _deleteFavorite;
        private readonly GetFavoriteById _getFavoriteById;

        public FavoritesController(
            CreateFavorite createFavorite,
            GetFavorite getFavorites,
            UpdateFavorite updateFavorite,
            DeleteFavorite deleteFavorite,
            GetFavoriteById getFavoriteById
        )
        {
            _createFavorite = createFavorite;
            _getFavorites = getFavorites;
            _updateFavorite = updateFavorite;
            _deleteFavorite = deleteFavorite;
            _getFavoriteById = getFavoriteById;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, Guid userId)
        {
            await _createFavorite.Execute(id, userId);
            return Ok(new { message = "Favorite added successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var services = await _getFavorites.Execute();
                return Ok(services);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var service = await _getFavoriteById.Execute(id);
                return Ok(service);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromQuery] Guid id, [FromQuery] Guid userId)
        {
            await _updateFavorite.Execute(id, userId);
            return Ok(new { message = "Favorite updated successfully" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteFavorite.Execute(id);
            return Ok(new { message = "Favorite deleted successfully" });
        }
    }

}
