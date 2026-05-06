using FirebaseAdmin.Messaging;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Infraestructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;

        public ReviewRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }
        public async Task CreateReviewAsync(ReviewDTO dto)
        {
            if (dto == null)
                throw new Exception("Datos inválidos");

            // 1. Crear review
            var review = new Reviews
            {
                client_id = dto.client_id,
                rating = dto.rating,
                comment = dto.comment,
                job_id = dto.id,
                is_active = true
            };

            // 2. Guardar imagen (si existe)
            if (dto.image_url != null && dto.image_url.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/reviews");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}.webp";
                var fullPath = Path.Combine(folder, fileName);

                await _imageService.SaveOptimizedImageAsync(dto.image_url, fullPath);

                review.image_url = "/img/reviews/" + fileName;
            }

            await _context.reviews.AddAsync(review);

            // 3. Buscar en express primero, luego en services
            Guid? workerId = null;

            var express = await _context.express
                .FirstOrDefaultAsync(x => x.express_id == dto.id);

            if (express != null)
            {
                workerId = express.worker_id;
            }
            else
            {
                var jobs = await _context.jobs.FirstOrDefaultAsync(x => x.job_id == dto.id);

                if (jobs != null)
                {
                    workerId = jobs.worker_id;
                }
                else
                {
                    throw new Exception("No se encontró el trabajo");
                }
            }

            // Validación extra (seguridad)
            if (workerId == null)
                throw new Exception("El trabajo no tiene worker asignado");

            // 4. Crear review del trabajador
            var reviewWorker = new Reviews_workers
            {
                client_id = dto.client_id,
                rating = dto.rating_worker,
                comment = dto.comment_worker,
                worker_id = workerId.Value,
                is_active = true
            };

            await _context.reviews_workers.AddAsync(reviewWorker);

            // 5. Guardar todo con manejo de error real
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<object?> GetReviewByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from r in _context.reviews
                where r.job_id == id && r.is_active == true
                select new
                {
                    r.job_id, r.rating, r.comment,
                    Image = string.IsNullOrEmpty(r.image_url) ? null : baseUrl + r.image_url,
                    client = (
                    from c in _context.users
                    where c.user_id == r.client_id
                    select new
                    {
                        c.username,
                        Image = string.IsNullOrEmpty(c.image_url)
                                ? null
                                : baseUrl + c.image_url
                    }
                    ).FirstOrDefault()
                    
                }).ToListAsync();
           

            if (result == null)
                return null;

            return result;
        }
        
        public async Task<object?> GetReviewWorkerByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from r in _context.reviews_workers
                join s in _context.workers on r.worker_id equals s.worker_id
                where r.worker_id == id
                select new
                {
                    r.worker_id,
                    r.rating,
                    r.comment,
                    client = (
                    from c in _context.users
                    where c.user_id == r.client_id
                    select new
                    {
                        c.username,
                        Image = string.IsNullOrEmpty(c.image_url)
                                ? null
                                : baseUrl + c.image_url
                    }
                    ).FirstOrDefault()

                }).ToListAsync();

            if (result == null)
                return null;

            return result;
        }
    }
}
