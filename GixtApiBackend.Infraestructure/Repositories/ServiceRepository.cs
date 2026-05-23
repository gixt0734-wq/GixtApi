using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

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
                // Construimos el servicio con los datos recibidos del DTO
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

                // Buscamos el trabajador asociado al usuario que crea el servicio
                var worker = (
                    from w in _context.workers
                    join u in _context.users
                        on w.user_id equals u.user_id
                    where u.user_id == dto.user_id
                    select w.worker_id
                ).FirstOrDefault();

                // Si hay trabajador, lo asignamos al servicio
                if (worker != null)
                    service.worker_id = worker;

                // Si llega la imagen principal, la guardamos y asignamos su URL
                if (dto.image != null && dto.image.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.image, "services");
                    service.image_url = img;
                }

                // Guardamos el servicio primero para tener su id disponible
                await _context.services.AddAsync(service);
                await _context.SaveChangesAsync();

                // Si llegan imágenes adicionales (galería), las procesamos
                if (dto.images != null && dto.images.Count > 0)
                {
                    var imagesToAdd = new List<Service_image>();

                    foreach (var img in dto.images)
                    {
                        // Saltamos imágenes nulas o vacías
                        if (img == null || img.Length == 0)
                            continue;

                        // Guardamos la imagen y obtenemos su URL
                        var imgs = await _imageService.SaveImageAsync(dto.image, "img_services");

                        // Creamos la imagen de galería asociada al servicio
                        imagesToAdd.Add(new Service_image
                        {
                            service_image_id = Guid.NewGuid(),
                            image_url = imgs,
                            service_id = service.service_id,
                            is_active = true
                        });
                    }

                    // Si se generaron imágenes, las insertamos en la base de datos
                    if (imagesToAdd.Count > 0)
                    {
                        await _context.serviceimages.AddRangeAsync(imagesToAdd);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Propagamos el error con un mensaje claro
                throw new Exception("Error creating the service: " + ex.Message, ex);
            }
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            try
            {
                // Buscamos el servicio por su id
                var existing = await _context.services.FindAsync(id);

                // Lo desactivamos (borrado lógico)
                existing.is_active = false;

                // Guardamos solo si el servicio existe
                if (existing != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al desactivar el servicio en la base de datos
                throw new Exception("Error deleting the service in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task UpdateServiceAsync(Service service)
        {
            try
            {
                // Buscamos el servicio a actualizar
                var existing = await _context.services.FindAsync(service.service_id);

                // Guardamos solo si el servicio existe
                if (existing != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al actualizar el servicio en la base de datos
                throw new Exception("Error updating the service in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetAllServicesAsync()
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos todos los servicios activos uniendo trabajador, usuario y categoría
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
                        // Imagen del usuario (trabajador)
                        UserImage = string.IsNullOrEmpty(u.image_url)
                            ? null
                            : baseUrl + u.image_url,
                        Category = c.name,
                        // Imagen del servicio
                        Image = string.IsNullOrEmpty(s.image_url)
                            ? null
                            : baseUrl + s.image_url
                    }
                ).ToListAsync();

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de servicios
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los servicios
                throw new Exception("Error retrieving the services list", ex);
            }
        }

        public async Task<object> GetAllServicesLocationAsync(decimal latitude, decimal longitude, double rangoKm)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Convertimos a double solo para los cálculos geográficos
                double lat = (double)latitude;
                double lon = (double)longitude;

                // Calculamos el rango aproximado en grados de latitud y longitud
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
                        // Filtramos en decimal para aprovechar el índice (caja delimitadora)
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
                        // Distancia real usando la fórmula de Haversine (en double)
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
                // Filtramos por distancia real, ordenamos y tomamos un servicio por categoría
                .Where(x => x.Distancia <= rangoKm)
                .OrderBy(x => x.Distancia)
                .OrderBy(x => x.rating)
                .GroupBy(x => x.category_id)
                .Select(g => g.First())
                .ToListAsync();

                // Devolvemos la lista de servicios cercanos
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los servicios por ubicación
                throw new Exception("Error retrieving the services by location", ex);
            }
        }

        public async Task<object?> GetServiceByIdAsync(Guid id, Guid userId)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el servicio activo con todos sus datos relacionados
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
                        // Tiempo relativo desde que se registró el servicio
                        registered = FechaHelper.GetTiempoRelativo(s.created_at),
                        // Datos del trabajador que ofrece el servicio
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
                        // Reseñas del servicio (a través de los trabajos realizados)
                        Review = (
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
                        // Imagen principal del servicio
                        Image = string.IsNullOrEmpty(s.image_url)
                            ? null
                            : baseUrl + s.image_url,
                        // Imágenes de la galería del servicio
                        Images = _context.serviceimages
                            .Where(img => img.service_id == s.service_id)
                            .Select(img => baseUrl + img.image_url)
                            .ToList(),
                        // Indica si el usuario actual tiene este servicio como favorito
                        favorite = _context.favorites
                            .Any(f =>
                                f.service_id == s.service_id &&
                                f.user_id == userId &&
                                f.is_active == true
                            )
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el servicio, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el servicio por id
                throw new Exception("Error retrieving the service by id", ex);
            }
        }

        public async Task<object?> GetServiceByIdWorkerAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos los servicios activos del trabajador indicado
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

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de servicios del trabajador
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los servicios del trabajador
                throw new Exception("Error retrieving the services by worker", ex);
            }
        }

        public async Task<object?> GetServicesByCategoryAsync(int id, decimal latitude, decimal longitude, double rangoKm, int pageNumber = 1)
        {
            try
            {
                // Tamaño fijo de página para la paginación
                const int pageSize = 10;

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Convertimos a double solo para los cálculos geográficos
                double lat = (double)latitude;
                double lon = (double)longitude;

                // Calculamos el rango aproximado en grados de latitud y longitud
                double rangoLat = rangoKm / 111.0;
                double rangoLon = rangoKm / (111.0 * Math.Cos(lat * Math.PI / 180));

                // Armamos la consulta base de servicios activos de la categoría dentro del rango
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
                        // Filtramos en decimal para aprovechar el índice (caja delimitadora)
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
                        // Distancia real usando la fórmula de Haversine (en double)
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

                // Contamos el total de elementos para la paginación
                var totalItems = await query.CountAsync();

                // Filtramos por distancia real, ordenamos y aplicamos paginación
                var result = await query
                    .Where(x => x.Distancia <= rangoKm)
                    .OrderBy(x => x.Distancia)
                    .OrderBy(x => x.rating)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Devolvemos los resultados junto con la metadata de paginación
                return new
                {
                    page = pageNumber,
                    pageSize = pageSize,
                    totalItems = totalItems,
                    hasMore = pageNumber * pageSize < totalItems,
                    data = result
                };
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los servicios por categoría
                throw new Exception("Error retrieving the services by category", ex);
            }
        }
    }
}