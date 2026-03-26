using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infrastructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Infrastructure.Repositories
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
            var dto = new Favorite();
            dto.is_active = true;
            dto.service_id = id;
            dto.user_id = userId;

            await _context.favorites.AddAsync(dto);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteFavoriteAsync(Guid id)
        {
            var favorite = await _context.favorites.FindAsync(id);
            if (favorite != null)
            {
                _context.favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateFavoriteAsync(Guid serviceId, Guid userId)
        {
            var existing = await _context.favorites
                .FirstOrDefaultAsync(f =>
                    f.service_id == serviceId &&
                    f.user_id == userId
                );

            if (existing != null)
            {
                existing.is_active = !existing.is_active; // toggle
                await _context.SaveChangesAsync();
            }
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

        public async Task<IEnumerable<Favorite>> GetAllFavoritesAsync()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await _context.favorites
                .Where(f => f.is_active == true)
                .ToListAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetFavoriteByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from f in _context.favorites
                join s in _context.services on f.service_id equals s.service_id
                join w in _context.workers on s.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                join c in _context.categories on s.category_id equals c.category_id
                where f.is_active == true && f.user_id == id && s.is_active == true && c.is_active == true && u.is_active == true
                select new
                {
                    s.service_id,
                    s.service_name,
                    s.labor_price,
                    u.first_name,
                    s.rating,
                    s.description,
                    UserImage = string.IsNullOrEmpty(u.image_url)
                           ? null
                           : baseUrl + u.image_url,
                    Category = c.name,
                    Image = string.IsNullOrEmpty(s.image_url)
                           ? null
                           : baseUrl + s.image_url
                }
           ).ToListAsync();

            if (result == null)
                return null;

            return result;
        }
    }

}
