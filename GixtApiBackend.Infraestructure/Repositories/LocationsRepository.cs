using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.Entities;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Application.DTos;


namespace GixtApiBackend.Infraestructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocationRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateLocationAsync(LocationDTO dto)
        {
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

            location.is_active = true;

            if (dto.image != null && dto.image.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/location");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.image.CopyToAsync(stream);
                }

                location.image_url = "/img/location/" + fileName;
            }

            await _context.locations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(Guid id)
        {
            var location = await _context.locations.FindAsync(id);
            if (location == null)
                throw new Exception("Location not found ");
            var existing = _context.jobs
                        .Any(j =>
                            j.location_id == id
                        );
            if (existing == true)
            {
                location.is_active = false;
                await _context.SaveChangesAsync();
               
            }
            else
            {
                if (!string.IsNullOrEmpty(location.image_url))
                {
                    var imagePath = location.image_url.TrimStart('/');

                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        imagePath
                    );

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                _context.locations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateLocationAsync(LocationDTO dto)
        {
            var existing = await _context.locations.FindAsync(dto.user_id);

            if (existing == null)
                throw new Exception("User not found");

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

            if (dto.image != null && dto.image.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/location");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                if (!string.IsNullOrEmpty(existing.image_url))
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existing.image_url.TrimStart('/')
                    );

                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.image.CopyToAsync(stream);
                }

                existing.image_url = "/img/location/" + fileName;
            }

            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            var locations = await _context.locations.ToListAsync();

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return locations;
        }

        public async Task<object?> GetLocationByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

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
                    Image = string.IsNullOrEmpty(l.image_url)
                            ? null
                            : baseUrl + l.image_url,
                    l.maps_address,
                    l.latitude,
                    l.longitude,
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetLocationsByUserIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

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
                    Image = string.IsNullOrEmpty(l.image_url)
                            ? null
                            : baseUrl + l.image_url,
                    l.maps_address,
                    l.latitude,
                    l.longitude,
                }
            ).ToListAsync();

            if (result == null)
                return null;

            return result;
        }
    }

}
