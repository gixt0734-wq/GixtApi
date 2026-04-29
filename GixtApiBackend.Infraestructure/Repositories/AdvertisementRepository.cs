using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;


namespace GixtApi.Infraestructure.Repositories
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdvertisementRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateAdvertisementAsync(AdvertisementDTO dto)
        {
            var advertisement = new Advertisement();
            advertisement.is_active = true;
            advertisement.name = dto.name;

            if (dto.image != null && dto.image.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/advertisements");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.image.CopyToAsync(stream);
                }

                // Guardar la ruta accesible desde la web
                advertisement.image_url = "/img/advertisements/" + fileName;
            }

            await _context.advertisements.AddAsync(advertisement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAdvertisementAsync(int id)
        {
            var advertisement = await _context.advertisements.FindAsync(id);

            if (advertisement != null)
            {
                _context.advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAdvertisementAsync(Advertisement advertisement)
        {
            var existing = await _context.advertisements.FindAsync(advertisement.advertisement_id);

            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Advertisement>> GetAllAdvertisementsAsync()
        {
            var advertisements = await _context.advertisements
                .Where(a => a.is_active == true)
                .ToListAsync();

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            foreach (var advertisement in advertisements)
            {
                if (!string.IsNullOrEmpty(advertisement.image_url))
                    advertisement.image_url = baseUrl + advertisement.image_url;
            }

            return advertisements;
        }
    }

}
