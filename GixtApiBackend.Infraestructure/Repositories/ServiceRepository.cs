using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using GixtApiBackend.Infraestructure;

namespace GixtApi.Infraestructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;

        public ServiceRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }

        public async Task CreateServiceAsync(ServiceDTO dto)
        {
            string? mainImagePath = null;

            try
            {
                var service = new Service
                {
                    service_name = dto.service_name,
                    description = dto.description,
                    labor_price = dto.labor_price,
                    category_id = dto.category_id,
                    duration_hours = dto.duration_hours,
                    is_active = true,
                    rating = 0
                };

                var worker = (
                      from w in _context.workers
                      join u in _context.users
                          on w.user_id equals u.user_id
                      where u.user_id == dto.user_id
                      select w.worker_id
                  ).FirstOrDefault();

                if (worker != null)
                    service.worker_id = worker;

                // ===============================
                // CARPETAS
                // ===============================

                var mainFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/services");
                var additionalFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/img_services");

                if (!Directory.Exists(mainFolder))
                    Directory.CreateDirectory(mainFolder);

                if (!Directory.Exists(additionalFolder))
                    Directory.CreateDirectory(additionalFolder);

                // ===============================
                // IMAGEN PRINCIPAL
                // ===============================

                if (dto.image != null && dto.image.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}.webp";
                    mainImagePath = Path.Combine(mainFolder, fileName);

                    await _imageService.SaveOptimizedImageAsync(dto.image, mainImagePath);

                    service.image_url = "/img/services/" + fileName;
                }

                // Guardar servicio primero
                await _context.services.AddAsync(service);
                await _context.SaveChangesAsync();

                // ===============================
                // IMÁGENES ADICIONALES
                // ===============================

                if (dto.images != null && dto.images.Count > 0)
                {
                    var imagesToAdd = new List<Service_image>();

                    foreach (var img in dto.images)
                    {
                        if (img == null || img.Length == 0)
                            continue;

                        var fileName = $"{Guid.NewGuid()}.webp";
                        var imagePath = Path.Combine(additionalFolder, fileName);

                        await _imageService.SaveOptimizedImageAsync(img, imagePath);

                        imagesToAdd.Add(new Service_image
                        {
                            service_image_id = Guid.NewGuid(),
                            image_url = "/img/img_services/" + fileName,
                            service_id = service.service_id,
                            is_active = true
                        });
                    }

                    if (imagesToAdd.Count > 0)
                    {
                        await _context.serviceimages.AddRangeAsync(imagesToAdd);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(mainImagePath) && File.Exists(mainImagePath))
                    File.Delete(mainImagePath);

                throw new Exception("Error al crear el servicio: " + ex.Message, ex);
            }
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            var existing = await _context.services.FindAsync(id);
            existing.is_active = false;
            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateServiceAsync(Service service)
        {
            var existing = await _context.services.FindAsync(service.service_id);
            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<object>> GetAllServicesAsync()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
               from s in _context.services
               join w in _context.workers on s.worker_id equals w.worker_id
               join u in _context.users on w.user_id equals u.user_id
               join c in _context.categories on s.category_id equals c.category_id
               where s.is_active == true && c.is_active == true && u.is_active == true
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

        public async Task<object> GetAllServicesLocationAsync(decimal latitude, decimal longitude,double rangoKm)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // 🔹 convertir a double SOLO para cálculos
            double lat = (double)latitude;
            double lon = (double)longitude;

            double rangoLat = rangoKm / 111.0;
            double rangoLon = rangoKm / (111.0 * Math.Cos(lat * Math.PI / 180));

            var result = await (
                from s in _context.services
                join w in _context.workers on s.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                join c in _context.categories on s.category_id equals c.category_id

                where s.is_active == true
                    && c.is_active == true
                    && u.is_active == true
                    && w.is_active == true
                    
                    // 🔹 AQUÍ se queda en decimal (usa índice)
                    && w.latitude >= latitude - (decimal)rangoLat
                    && w.latitude <= latitude + (decimal)rangoLat
                    && w.longitude >= longitude - (decimal)rangoLon
                    && w.longitude <= longitude + (decimal)rangoLon

                select new
                {
                    s.service_id,
                    s.service_name,
                    s.labor_price,
                    u.first_name,
                    s.rating,
                    s.description,
                    s.category_id,
                    w.city,
                    // 🔥 convertir a double dentro del cálculo
                    Distancia = 6371 * 2 * Math.Asin(Math.Sqrt(
                        Math.Pow(Math.Sin((lat - (double)w.latitude) * Math.PI / 180 / 2), 2) +
                        Math.Cos(lat * Math.PI / 180) *
                        Math.Cos((double)w.latitude * Math.PI / 180) *
                        Math.Pow(Math.Sin((lon - (double)w.longitude) * Math.PI / 180 / 2), 2)
                    )),

                    UserImage = string.IsNullOrEmpty(u.image_url)
                        ? null
                        : baseUrl + u.image_url,

                    Category = c.name,

                    Image = string.IsNullOrEmpty(s.image_url)
                        ? null
                        : baseUrl + s.image_url
                }
            )
            .Where(x => x.Distancia <= rangoKm)
            .OrderBy(x => x.Distancia)
            .OrderBy(x => x.rating)
            .GroupBy(x => x.category_id)
            .Select(g => g.First())
            .ToListAsync();

            return result;
        }

        public async Task<object?> GetServiceByIdAsync(Guid id, Guid userId)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from s in _context.services
                join w in _context.workers on s.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                join c in _context.categories on s.category_id equals c.category_id
                where s.is_active == true && s.service_id == id && u.is_active == true
                select new
                {
                    s.service_id,
                    s.service_name,
                    s.labor_price,
                    s.rating,
                    s.description,
                    s.duration_hours,
                    registered = FechaHelper.GetTiempoRelativo(s.created_at),

                    Worker = (
                        from w in _context.workers
                        join u in _context.users
                            on w.user_id equals u.user_id
                        where w.worker_id == s.worker_id
                        select new
                        {
                            u.user_id,
                            u.first_name,
                            u.username,
                            u.email,
                            w.description,
                            w.city,
                            w.rating,
                            w.km_cost,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),

                    Review =( 
                     from j in _context.jobs
                     join r in _context.reviews on j.job_id equals r.job_id
                     where j.service_id == s.service_id
                     select new
                     {
                         r.job_id,
                         r.rating,
                         r.comment,
                         registrado = FechaHelper.GetTiempoRelativo(r.created_at),
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

                     }).ToList(),
                    Category = c.name,

                    Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url,

                    Images = _context.serviceimages
                        .Where(img => img.service_id == s.service_id)
                        .Select(img => baseUrl + img.image_url)
                        .ToList(),

                    favorite = _context.favorites
                        .Any(f =>
                            f.service_id == s.service_id &&
                            f.user_id == userId &&
                            f.is_active == true
                        )
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetServiceByIdWorkerAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from s in _context.services
                join w in _context.workers on s.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                join c in _context.categories on s.category_id equals c.category_id
                where s.is_active == true && u.is_active == true && w.user_id == id
                select new
                {
                    s.service_id,
                    s.service_name,
                    s.labor_price,
                    u.first_name,
                    s.rating,
                    w.city,
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

        public async Task<object?> GetServicesByCategoryAsync(int id, decimal latitude, decimal longitude, double rangoKm, int pageNumber = 1)
        {
            const int pageSize = 10;

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            double lat = (double)latitude;
            double lon = (double)longitude;

            double rangoLat = rangoKm / 111.0;
            double rangoLon = rangoKm / (111.0 * Math.Cos(lat * Math.PI / 180));

            var query =
                from s in _context.services
                join w in _context.workers on s.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                join c in _context.categories on s.category_id equals c.category_id
                 where s.is_active == true
                    && c.is_active == true
                    && u.is_active == true
                    && w.is_active == true
                    && s.category_id == id
                    // 🔹 AQUÍ se queda en decimal (usa índice)
                    && w.latitude >= latitude - (decimal)rangoLat
                    && w.latitude <= latitude + (decimal)rangoLat
                    && w.longitude >= longitude - (decimal)rangoLon
                    && w.longitude <= longitude + (decimal)rangoLon
                select new
                {
                    s.service_id,
                    s.service_name,
                    s.labor_price,
                    u.first_name,
                    s.rating,
                    s.description,
                    w.city,
                    Distancia = 6371 * 2 * Math.Asin(Math.Sqrt(
                        Math.Pow(Math.Sin((lat - (double)w.latitude) * Math.PI / 180 / 2), 2) +
                        Math.Cos(lat * Math.PI / 180) *
                        Math.Cos((double)w.latitude * Math.PI / 180) *
                        Math.Pow(Math.Sin((lon - (double)w.longitude) * Math.PI / 180 / 2), 2)
                    )),

                    UserImage = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url,
                    Category = c.name,
                    Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                };

            var totalItems = await query.CountAsync();

            var result = await query
                .Where(x => x.Distancia <= rangoKm)
                .OrderBy(x => x.Distancia)
                .OrderBy(x => x.rating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return new
            {
                page = pageNumber,
                pageSize = pageSize,
                totalItems = totalItems,
                hasMore = pageNumber * pageSize < totalItems,
                data = result
            };
        }
    }

}
