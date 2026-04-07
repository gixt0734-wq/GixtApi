using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infrastructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http.HttpResults;


namespace GixtApiBackend.Infrastructure.Repositories
{
    public class ExpressRepository : IExpressRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public ExpressRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task<Guid> CreateExpressAsync(ExpressDTO dto)
        {
            var job = new Express
            {
                client_id = dto.client_id,
                job_date = dto.job_date,
                job_time = dto.job_time,
                description = dto.description,
                category_id = dto.category_id,
                problem = dto.problem,
                maps_address = dto.maps_address,
                latitude = dto.latitude,
                longitude = dto.longitude,
            };

            job.is_active = true;
            job.job_status = "pending";
         
            if (dto.image != null && dto.image.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/jobs_express/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}.webp";
                var fullPath = Path.Combine(folder, fileName);

                await _imageService.SaveOptimizedImageAsync(dto.image, fullPath);

                job.image_url = "/img/jobs_express/" + fileName;
            }

            
            
            await _context.express.AddAsync(job);

            var payment = new Payment
            {
                job_id = job.express_id,
                payment_method = dto.payment_method,
                payment_status = "pending"
            };

            await _context.payment.AddAsync(payment);
            await _context.SaveChangesAsync();

            await _fcmService.SendNotificationByExpress(
                   job.category_id,
                   "Alguien necesita tu ayuda",
                   "Tienes un nuevo servicio express, verificalo",
                   job.express_id
               );
            return job.express_id;
        }

        public async Task<IEnumerable<Express>> GetAllExpresssAsync()
        {
            var jobs = await _context.express.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return jobs;
        }

        public async Task<object> GetExpressReviewIdAsync (Guid id, Guid idworker)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from e in _context.express
                where e.is_active == true && e.express_id == id
                select new
                {
                    e.express_id,
                    e.client_id,
                    Client = (
                        from u in _context.users
                        where u.user_id == e.client_id
                        select new
                        {
                            u.first_name,
                            u.username,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),
                    Worker = (
                        from w in _context.workers 
                        where w.user_id == idworker
                        select new
                        {
                          w.km_cost
                        }
                    ).FirstOrDefault(),
                    payment = _context.payment
                        .Where(c => c.job_id == e.express_id)
                        .Select(c => new
                        {
                            c.materials,
                            c.labor_cost,
                            c.km_cost,
                            c.payment_method,
                            c.payment_status,
                            c.iva,
                            c.total
                        }).FirstOrDefault(),

                    e.job_date,
                    e.job_time,
                    e.latitude,
                    e.longitude,
                    e.maps_address,
                    e.description,
                    e.problem,
                    e.is_active,
                    e.job_status,

                    Image= string.IsNullOrEmpty(e.image_url)
                        ? null
                        : baseUrl + e.image_url,

                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetExpressByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from e in _context.express
                where e.is_active == true && e.express_id == id
                select new
                {
                    e.express_id,
                    e.client_id,
                    Worker = (
                        from s in _context.express
                        join w in _context.workers on s.worker_id equals w.worker_id
                        join u in _context.users on w.user_id equals u.user_id
                        where s.express_id== e.express_id
                        select new
                        {
                            u.first_name,
                            u.username,
                            w.rating,
                            w.description,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),

                    e.job_date,
                    e.job_time,
                    e.latitude,
                    e.longitude,
                    e.maps_address,
                    e.description,
                    e.problem,
                    e.is_active,
                    e.job_status,

                    Image = string.IsNullOrEmpty(e.image_url)
                        ? null
                        : baseUrl + e.image_url,

                    
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }


        //public async Task<object?> GetExpressWorkerByIdAsync(Guid id)
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    var baseUrl = $"{request.Scheme}://{request.Host}";

        //    var result = await (
        //        from t in _context.jobs
        //        where t.is_active == true && t.job_id == id
        //        select new
        //        {
        //            t.job_id,
        //            t.client_id,

        //            Client = (
        //                from  u in _context.users 
        //                where u.user_id== t.client_id
        //                select new
        //                {
        //                    u.first_name,
        //                    u.username,
        //                    Image = string.IsNullOrEmpty(u.image_url)
        //                        ? null
        //                        : baseUrl + u.image_url
        //                }
        //            ).FirstOrDefault(),

        //            location = _context.locations
        //                .Where(l => l.location_id == t.location_id)
        //                .Select(l => new
        //                {
        //                    l.location_id,
        //                    l.street,
        //                    l.neighborhood,
        //                    l.house_number,
        //                    l.state,
        //                    l.maps_address,
        //                    l.reference,
        //                    l.longitude,
        //                    l.latitude,
        //                    Image = string.IsNullOrEmpty(l.image_url)
        //                        ? null
        //                        : baseUrl + l.image_url,
        //                }).FirstOrDefault(),

        //            service = _context.services
        //                .Where(s => s.service_id == t.service_id)
        //                .Select(s => new
        //                {
        //                    s.service_id,
        //                    s.service_name,
        //                    s.description,
        //                    Image = string.IsNullOrEmpty(s.image_url)
        //                        ? null
        //                        : baseUrl + s.image_url
        //                }).FirstOrDefault(),

        //            t.job_date,
        //            t.job_time,
        //            t.description,
        //            t.problem,
        //            t.price,
        //            t.payment_method,
        //            t.terms,
        //            t.is_active,
        //            t.job_status,
        //            t.payment_status,

        //            Image_1 = string.IsNullOrEmpty(t.image_1_url)
        //                ? null
        //                : baseUrl + t.image_1_url,

        //            Image_2 = string.IsNullOrEmpty(t.image_2_url)
        //                ? null
        //                : baseUrl + t.image_2_url,
        //        }
        //    ).FirstOrDefaultAsync();

        //    if (result == null)
        //        return null;

        //    return result;
        //}

        //public async Task<object?> GetExpresssByUserIdAsync(Guid userId)
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    var baseUrl = $"{request.Scheme}://{request.Host}";

        //    var result = await (
        //        from t in _context.jobs
        //        where t.is_active == true && t.client_id == userId
        //        select new
        //        {
        //            t.job_id,

        //            Worker = (
        //                from s in _context.services
        //                join w in _context.workers on s.worker_id equals w.worker_id
        //                join u in _context.users on w.user_id equals u.user_id
        //                where s.service_id == t.service_id
        //                select new
        //                {
        //                    u.user_id,
        //                    u.first_name,
        //                    u.username,
        //                    Image = string.IsNullOrEmpty(u.image_url)
        //                        ? null
        //                        : baseUrl + u.image_url
        //                }
        //            ).FirstOrDefault(),

        //            t.client_id,

        //            location = _context.locations
        //                .Where(l => l.location_id == t.location_id)
        //                .Select(l => new
        //                {
        //                    l.maps_address,
        //                }).FirstOrDefault(),

        //            service = _context.services
        //                .Where(s => s.service_id == t.service_id)
        //                .Select(s => new
        //                {
        //                    s.service_id,
        //                    s.service_name,
        //                    s.description,
        //                    Image = string.IsNullOrEmpty(s.image_url)
        //                        ? null
        //                        : baseUrl + s.image_url
        //                }).FirstOrDefault(),

        //            t.job_date,
        //            t.job_time,
        //            t.description,
        //            t.problem,
        //            t.is_active,
        //            t.price,
        //            t.job_status,
        //        }
        //    ).ToListAsync();

        //    if (result == null)
        //        return null;

        //    return result;
        //}

        public async Task<object?> GetExpresssByWorkerIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from t in _context.express
                join w in _context.workers on t.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                where t.is_active == true && w.user_id == id
                select new
                {
                    t.express_id,
                    t.problem,
                    t.description,
                    Image = string.IsNullOrEmpty(t.image_url)
                        ? null
                        : baseUrl + t.image_url,
                    Client = (
                        from s in _context.users
                        where s.user_id == t.client_id
                        select new
                        {
                            s.user_id,
                            s.first_name,
                            s.username,
                            Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                        }
                    ).FirstOrDefault(),
                    t.job_date,
                    t.job_time,
                    t.is_active,
                    t.job_status,
                    t.maps_address
                }
            ).ToListAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task DeleteExpressAsync(Guid id)
        {
            var job = await _context.express.FindAsync(id);
            var payment = _context.payment
                .Where(p => p.job_id == job.express_id)
                .FirstOrDefault();
            if (job == null)
            {
                return;
            }
            var user = await _context.users.FindAsync(job.client_id);
            if (job != null)
            {

                _context.express.Remove(job);
                _context.payment.Remove(payment);
                if (!string.IsNullOrEmpty(job.image_url))
                {
                    var imagePath = job.image_url.TrimStart('/');

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
                await _context.SaveChangesAsync();

                if(job.job_status != "pending")
                await _fcmService.SendNotificationByWorker(
                    job.worker_id.Value,
                     "Servicio Express Cancelado",
                     $"{user.username} ha cancelado el servicio express que habías aceptado.",
                     "cancelation_express"
                 );
            }
        }

        public async Task CancelExpressAsync(Guid id)
        {
            var existing = await _context.express.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            existing.job_status = "accepted";
            var workerData = await (
             from u in _context.users
             join w in _context.workers on u.user_id equals w.user_id
             where u.user_id == existing.worker_id
             select new
             {
                 Username = u.username,
                 WorkerId = w.worker_id
             }).FirstOrDefaultAsync();

            if (workerData == null)
                return;

            await _fcmService.SendNotificationByUser(
                existing.client_id,
                "El cliente a aceptado",
                $"El servicio express '{existing.problem}' fue accepto tu solicitud, ve rapido.", "Express"
            );

            await _context.SaveChangesAsync();
        }

        public async Task SendAcceptAsync(Guid worker, Guid id, decimal km_cost , decimal labor_price)
        {
            var service = await _context.express.FindAsync(id);
         
            if (service == null)
                return;
            
            var workerData = await (
                from u in _context.users
                join w in _context.workers on u.user_id equals w.user_id
                where u.user_id == worker
                select new
                {
                    Username = u.username,
                    WorkerId = w.worker_id
                }
            ).FirstOrDefaultAsync();

            if (workerData == null)
                return;

            await _fcmService.sendNotificationByExpress(
                id,
                workerData.Username,
                workerData.WorkerId,
                $"{workerData.Username} a enviado una propuesta.",
                $"{workerData.Username} realiza el trabajo por: {labor_price} y por ir ${km_cost}",
                labor_price,km_cost
                
            );
        }

        public async Task AcceptExpressAsync(Guid express_id , Guid worker_id, decimal km_cost, decimal labor_price)
        {
            var existing = await _context.express.FindAsync(express_id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            existing.job_status = "accepted";
            existing.worker_id = worker_id;

            var existingpay = _context.payment
                .Where(p => p.job_id == express_id)
                .FirstOrDefault();

            if (existingpay != null)
            {
                existingpay.km_cost = km_cost;
                existingpay.labor_cost = labor_price;

            }

            await _fcmService.SendNotificationByWorker(
                worker_id,
                "El cliente a aceptado",
                $"El servicio express '{existing.problem}' fue accepto tu solicitud, ve rapido.", "Express"
            );

            await _context.SaveChangesAsync();
        }


        public async Task UpdateExpressStatusAsync(Guid id, string action)
        {

            var existing = await _context.express.FindAsync(id);
            

            if (existing == null)
                throw new Exception("Trabajo no encontrado");
            
            switch (action)
            {
                
                case "pending":

                    existing.job_status = "accepted";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio aceptado ",
                        $"El servicio '{existing.problem}' fue aceptado correctamente.", "Express"
                    );

                    break;
                case "accepted":

                    existing.job_status = "going";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "El trabajador esta llendo ",
                        $"El trabajador de '{existing.problem}' esta llendo a tu domicilio.", "Express"
                    );

                    break;
                case "going":

                    existing.job_status = "arrived";
                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "El trabajador ya llego",
                        $"El trabajador de '{existing.problem}' ya llego a tu domicilio.","Express"
                    );

                    break;
                case "arrived":

                    var now = DateTime.Now;

                    // Convertir DateOnly + TimeOnly → DateTime
                    var jobDateTime = existing.job_date.ToDateTime(existing.job_time);

                    // Validar: mismo día y máximo 30 minutos antes
                    if (now < jobDateTime.AddMinutes(-30) || now.Date != jobDateTime.Date)
                        throw new Exception("Solo puedes iniciar el servicio el mismo día o 30 minutos antes de la hora programada.");

                    existing.job_status = "in_progress";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio en progreso 🚀",
                        $"El trabajador ha iniciado el servicio '{existing.problem}'.", "Express"
                    );

                    break;

                case "in_progress":

                    existing.job_status = "finalized";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio finalizado 🎉",
                        $"El servicio '{existing.problem}' ha sido completado.", "Express"
                    );

                    break;

                case "canceled":

                    existing.job_status = "canceled";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio cancelado ❌",
                        $"El servicio '{existing.problem}' fue cancelado por el trabajador.", "Express"
                    );

                    break;

                default:
                    throw new Exception("Acción no válida");
            }

            await _context.SaveChangesAsync();
        }




    }

}
