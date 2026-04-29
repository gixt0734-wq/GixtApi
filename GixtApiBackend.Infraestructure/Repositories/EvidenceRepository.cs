using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;


namespace GixtApiBackend.Infraestructure.Repositories
{
    public class EvidenceRepository : IEvidenceRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public EvidenceRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task CreateEvidenceAsync(EvidenceDTO dto)
        {
            
          
            if (dto.images == null || dto.images.Count == 0)
                return;

            var additionalFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/img_evidences");


            if (!Directory.Exists(additionalFolder))
                Directory.CreateDirectory(additionalFolder);

            var evidencesToAdd = new List<Evidence>();

            foreach (var e in dto.images)
            {
                if (e == null)
                    continue;
                var fileName = $"{Guid.NewGuid()}.webp";
                var imagePath = Path.Combine(additionalFolder, fileName);

                await _imageService.SaveOptimizedImageAsync(e, imagePath);

                evidencesToAdd.Add( new Evidence
                {
                    image_url = "/img/img_evidences/" + fileName,
                    job_id = dto.job_id

                });
            }

            if (evidencesToAdd.Count > 0)
            {
                await _context.evidence.AddRangeAsync(evidencesToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Payment>> GetAllCostAsync()
        {
            var cost = await _context.payment.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return cost;
        }


        public async Task<object?> GetCostByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from c in _context.payment
                select new
                {
                    c.materials,
                    c.job_id,
                    c.payment_id,
                    c.labor_cost,
                    c.km_cost,
                    c.iva,
                    List_Materials = _context.materials
                        .Where(m => m.payment_id == c.payment_id)
                        .ToList(),

                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

    }

}
