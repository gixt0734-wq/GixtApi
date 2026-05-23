using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Entities;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace GixtApiBackend.Infraestructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;

        public LocationRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }

        public async Task CreateLocationAsync(LocationDTO dto)
        {
            try
            {
                // Construimos la ubicación con los datos recibidos del DTO
                var location = new Location
                {
                    user_id = dto.user_id,
                    street = dto.street,
                    neighborhood = dto.neighborhood,
                    house_number = dto.house_number,
                    state = dto.state,
                    city = dto.city,
                    reference = dto.reference,
                    maps_address = dto.maps_address,
                    latitude = dto.latitude,
                    longitude = dto.longitude,
                    country = dto.country,
                };

                // La ubicación inicia activa
                location.is_active = true;

                // Si llega una imagen, la guardamos y asignamos su URL
                if (dto.image != null && dto.image.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.image, "location");
                    location.image_url = img;
                }

                // Registramos la ubicación y guardamos en la base de datos
                await _context.locations.AddAsync(location);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir la ubicación en la base de datos
                throw new Exception("Error creating the location in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task DeleteLocationAsync(Guid id)
        {
            try
            {
                // Buscamos la ubicación por su id
                var location = await _context.locations.FindAsync(id);
                if (location == null)
                    throw new InvalidOperationException("Location not found");

                // Verificamos si hay trabajos que usan esta ubicación
                var existing = _context.jobs
                    .Any(j =>
                        j.location_id == id
                    );

                // Si la ubicación está en uso, solo la desactivamos (borrado lógico)
                if (existing == true)
                {
                    location.is_active = false;
                    await _context.SaveChangesAsync();
                }
                // Si no está en uso, la eliminamos físicamente junto con su imagen
                else
                {
                    if (!string.IsNullOrEmpty(location.image_url))
                    {
                        _imageService.DeleteImageAsync(location.image_url);
                    }
                    _context.locations.Remove(location);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al eliminar o desactivar la ubicación en la base de datos
                throw new Exception("Error deleting the location in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task UpdateLocationAsync(LocationDTO dto)
        {
            try
            {
                // Buscamos la ubicación a actualizar
                var existing = await _context.locations.FindAsync(dto.user_id);

                if (existing == null)
                    throw new InvalidOperationException("User not found");

                // Actualizamos solo los campos que llegan con valor (actualización parcial)
                if (!string.IsNullOrEmpty(dto.street))
                    existing.street = dto.street;

                if (!string.IsNullOrEmpty(dto.house_number))
                    existing.neighborhood = dto.house_number;

                if (!string.IsNullOrEmpty(dto.house_number))
                    existing.house_number = dto.house_number;

                if (!string.IsNullOrEmpty(dto.state))
                    existing.state = dto.state;

                if (!string.IsNullOrEmpty(dto.city))
                    existing.city = dto.city;

                if (!string.IsNullOrEmpty(dto.reference))
                    existing.reference = dto.reference;

                if (dto.longitude != null)
                    existing.longitude = dto.longitude;

                if (dto.latitude != null)
                    existing.latitude = dto.latitude;

                if (!string.IsNullOrEmpty(dto.maps_address))
                    existing.maps_address = dto.maps_address;

                // Si llega una nueva imagen, reemplazamos la anterior
                if (dto.image != null && dto.image.Length > 0)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/location");

                    // Creamos la carpeta destino si no existe
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    // Si ya había una imagen, la eliminamos físicamente del servidor
                    if (!string.IsNullOrEmpty(existing.image_url))
                    {
                        _imageService.DeleteImageAsync(existing.image_url);
                    }

                    var img = await _imageService.SaveImageAsync(dto.image, "location");
                    existing.image_url = img;
                }

                // Guardamos los cambios en la base de datos
                if (existing != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al actualizar la ubicación en la base de datos
                throw new Exception("Error updating the location in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            try
            {
                // Obtenemos todas las ubicaciones de la base de datos
                var locations = await _context.locations.ToListAsync();

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Devolvemos la lista de ubicaciones
                return locations;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar las ubicaciones
                throw new Exception("Error retrieving the locations list", ex);
            }
        }

        public async Task<object?> GetLocationByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos la ubicación activa que coincide con el id
                var result = await (
                    from l in _context.locations
                    where l.is_active == true && l.location_id == id
                    select new
                    {
                        l.user_id,
                        l.location_id,
                        l.street,
                        l.neighborhood,
                        l.house_number,
                        l.state,
                        l.city,
                        l.reference,
                        // URL de la imagen de la ubicación
                        Image = string.IsNullOrEmpty(l.image_url)
                            ? null
                            : baseUrl + l.image_url,
                        l.maps_address,
                        l.latitude,
                        l.longitude,
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró la ubicación, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar la ubicación por id
                throw new Exception("Error retrieving the location by id", ex);
            }
        }

        public async Task<object?> GetLocationsByUserIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos las ubicaciones activas del usuario indicado
                var result = await (
                    from l in _context.locations
                    where l.is_active == true && l.user_id == id
                    select new
                    {
                        l.user_id,
                        l.location_id,
                        l.street,
                        l.neighborhood,
                        l.house_number,
                        l.state,
                        l.city,
                        l.reference,
                        // URL de la imagen de la ubicación
                        Image = string.IsNullOrEmpty(l.image_url)
                            ? null
                            : baseUrl + l.image_url,
                        l.maps_address,
                        l.latitude,
                        l.longitude,
                    }
                ).ToListAsync();

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de ubicaciones
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar las ubicaciones del usuario
                throw new Exception("Error retrieving the locations by user", ex);
            }
        }
    }
}