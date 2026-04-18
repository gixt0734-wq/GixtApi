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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public PaymentRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task UpdatePaymentAsync(PaymentDtos dto)
        {

            var existing =  _context.payment
                .Where(p => p.job_id == dto.job_id)
                .FirstOrDefault();

            if (dto.isexpress)
            {
                var express = _context.express
                .Where(p => p.express_id == dto.job_id)
                .FirstOrDefault();
            }
            else
            {
                var job = _context.jobs
                .Where(p => p.job_id == dto.job_id)
                .FirstOrDefault();

                job.description_worker = dto.description;

                
            }

            if (existing != null)
            {
                existing.materials = dto.materials;
                existing.total = dto.total;
                existing.iva = dto.iva;
                existing.labor_cost = dto.labor_cost;
              
            }

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
                    payment_id = existing.payment_id
                });
            }

            if (materialsToAdd.Count > 0)
            {
                await _context.materials.AddRangeAsync(materialsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentAsync()
        {
            var cost = await _context.payment.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return cost;
        }


        public async Task<object?> GetPaymentByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from c in _context.payment 
                where c.job_id == id
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
