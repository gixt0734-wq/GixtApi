using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Infraestructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavoriteRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateFavoriteAsync(Guid id, Guid userId)
        {
            try
            {
                // Construimos el favorito con el servicio y usuario indicados
                var dto = new Favorite();
                dto.is_active = true;
                dto.service_id = id;
                dto.user_id = userId;

                // Registramos el favorito y guardamos en la base de datos
                await _context.favorites.AddAsync(dto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir el favorito en la base de datos
                throw new Exception("Error creating the favorite in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }
        public async Task DeleteFavoriteAsync(Guid id)
        {
            try
            {
                // Buscamos el favorito por su id
                var favorite = await _context.favorites.FindAsync(id);

                // Solo eliminamos si el favorito existe
                if (favorite != null)
                {
                    _context.favorites.Remove(favorite);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al eliminar el favorito de la base de datos
                throw new Exception("Error deleting the favorite from the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task UpdateFavoriteAsync(Guid serviceId, Guid userId)
        {
            try
            {
                // Buscamos si ya existe un favorito para este servicio y usuario
                var existing = await _context.favorites
                    .FirstOrDefaultAsync(f =>
                        f.service_id == serviceId &&
                        f.user_id == userId
                    );

                // Si ya existe, alternamos su estado activo/inactivo (toggle)
                if (existing != null)
                {
                    existing.is_active = !existing.is_active; // toggle
                    await _context.SaveChangesAsync();
                }
                // Si no existe, lo creamos como favorito activo
                else
                {
                    var dto = new Favorite();
                    dto.is_active = true;
                    dto.service_id = serviceId;
                    dto.user_id = userId;

                    await _context.favorites.AddAsync(dto);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al actualizar o crear el favorito en la base de datos
                throw new Exception("Error updating the favorite in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Favorite>> GetAllFavoritesAsync()
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Obtenemos todos los favoritos que están activos
                var result = await _context.favorites
                    .Where(f => f.is_active == true)
                    .ToListAsync();

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de favoritos
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los favoritos
                throw new Exception("Error retrieving the favorites list", ex);
            }
        }

        public async Task<object?> GetFavoriteByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos los servicios favoritos del usuario uniendo servicio, trabajador, usuario y categoría
                var result = await (
                    from f in _context.favorites
                    join s in _context.services on f.service_id equals s.service_id
                    join w in _context.workers on s.worker_id equals w.worker_id
                    join u in _context.users on w.user_id equals u.user_id
                    join c in _context.categories on s.category_id equals c.category_id
                    // Solo traemos favoritos activos cuyo servicio, categoría y usuario también estén activos
                    where f.is_active == true && f.user_id == id && s.is_active == true && c.is_active == true && u.is_active == true
                    select new
                    {
                        s.service_id,
                        s.service_name,
                        s.labor_price,
                        u.first_name,
                        s.rating,
                        s.description,
                        w.city,
                        // URL de la imagen del usuario (trabajador)
                        UserImage = string.IsNullOrEmpty(u.image_url)
                               ? null
                               : baseUrl + u.image_url,
                        Category = c.name,
                        // URL de la imagen del servicio
                        Image = string.IsNullOrEmpty(s.image_url)
                               ? null
                               : baseUrl + s.image_url
                    }
                ).ToListAsync();

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de servicios favoritos
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los favoritos por usuario
                throw new Exception("Error retrieving the favorites by user", ex);
            }
        }
    }

}
