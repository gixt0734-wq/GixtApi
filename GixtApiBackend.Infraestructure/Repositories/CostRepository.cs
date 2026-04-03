using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infrastructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;


namespace GixtApiBackend.Infrastructure.Repositories
{
    public class CostRepository : ICostRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public CostRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task CreateCostAsync(CostsDtos dto)
        {
            var costs = new Costs
            {
                job_id = dto.job_id,
                materials = dto.materials,
                labor_cost = dto.labor_cost,
                km_cost = dto.km_cost,
                iva = dto.iva,
                total = dto.total
            };

                

            await _context.costs.AddAsync(costs);
            await _context.SaveChangesAsync();

          
            if (dto.materiales == null || dto.materiales.Count == 0)
                return;

            var materialsToAdd = new List<Materials>();

            foreach (var mat in dto.materiales)
            {
                if (mat == null || string.IsNullOrWhiteSpace(mat.name))
                    continue;

                materialsToAdd.Add(new Materials
                {
                    name = mat.name,
                    cost = mat.cost,
                    cost_id = costs.cost_id
                });
            }

            if (materialsToAdd.Count > 0)
            {
                await _context.materials.AddRangeAsync(materialsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Costs>> GetAllCostAsync()
        {
            var cost = await _context.costs.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return cost;
        }


        public async Task<object?> GetCostByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from c in _context.costs 
                where c.job_id == id
                select new
                {
                    c.materials,
                    c.job_id,
                    c.cost_id,
                    c.labor_cost,
                    c.km_cost,
                    c.iva,
                    List_Materials = _context.materials
                        .Where(m => m.cost_id == c.cost_id)
                        .ToList(),

                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

    }

}
