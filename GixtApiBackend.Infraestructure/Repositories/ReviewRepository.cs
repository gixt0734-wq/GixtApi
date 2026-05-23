using FirebaseAdmin.Messaging;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;
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
            try
            {
                // Validamos que lleguen datos
                if (dto == null)
                    throw new ArgumentException("Invalid data");

                // 1. Creamos la reseña del servicio
                var review = new Reviews
                {
                    client_id = dto.client_id,
                    rating = dto.rating,
                    comment = dto.comment,
                    job_id = dto.id,
                    is_active = true
                };

                // 2. Si llega una imagen, la guardamos y asignamos su URL
                if (dto.image_url != null && dto.image_url.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.image_url, "reviews");
                    review.image_url = img;
                }

                // Registramos la reseña del servicio en el contexto
                await _context.reviews.AddAsync(review);

                // 3. Buscamos el trabajador: primero en express, luego en jobs
                Guid? workerId = null;

                var express = await _context.express
                    .FirstOrDefaultAsync(x => x.express_id == dto.id);

                // Si el trabajo es express, tomamos su trabajador
                if (express != null)
                {
                    workerId = express.worker_id;
                }
                else
                {
                    // Si no es express, lo buscamos como trabajo normal
                    var jobs = await _context.jobs.FirstOrDefaultAsync(x => x.job_id == dto.id);

                    if (jobs != null)
                    {
                        workerId = jobs.worker_id;
                    }
                    else
                    {
                        // No existe ni como express ni como trabajo normal
                        throw new InvalidOperationException("Job not found");
                    }
                }

                // Validación extra de seguridad: el trabajo debe tener trabajador asignado
                if (workerId == null)
                    throw new InvalidOperationException("The job has no worker assigned");

                // 4. Creamos la reseña del trabajador
                var reviewWorker = new Reviews_workers
                {
                    client_id = dto.client_id,
                    rating = dto.rating_worker,
                    comment = dto.comment_worker,
                    worker_id = workerId.Value,
                    is_active = true
                };

                await _context.reviews_workers.AddAsync(reviewWorker);

                // 5. Guardamos ambas reseñas con manejo de error específico
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Propagamos el error real de la base de datos (inner exception si existe)
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<object?> GetReviewByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos las reseñas activas del trabajo indicado
                var result = await (
                    from r in _context.reviews
                    where r.job_id == id && r.is_active == true
                    select new
                    {
                        r.job_id,
                        r.rating,
                        r.comment,
                        // URL de la imagen de la reseña
                        Image = string.IsNullOrEmpty(r.image_url) ? null : baseUrl + r.image_url,
                        // Datos básicos del cliente que dejó la reseña
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

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de reseñas
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar las reseñas del trabajo
                throw new Exception("Error retrieving the reviews by job", ex);
            }
        }

        public async Task<object?> GetReviewWorkerByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos las reseñas del trabajador indicado (join con workers)
                var result = await (
                    from r in _context.reviews_workers
                    join s in _context.workers on r.worker_id equals s.worker_id
                    where r.worker_id == id
                    select new
                    {
                        r.worker_id,
                        r.rating,
                        r.comment,
                        // Datos básicos del cliente que dejó la reseña
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

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de reseñas del trabajador
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar las reseñas del trabajador
                throw new Exception("Error retrieving the reviews by worker", ex);
            }
        }
    }
}