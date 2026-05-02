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
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public JobRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task CreateJobAsync(JobDTO dto)
        {
            var job = new Job
            {
                client_id = dto.client_id,
                service_id = dto.service_id,
                location_id = dto.location_id,
                job_date = dto.job_date,
                job_time = dto.job_time,
                description = dto.description,
                problem = dto.problem,
             
            };


            job.job_status = "pending";
            job.is_active = true;

            var service = _context.services
              .Where(s => s.service_id == job.service_id)
              .FirstOrDefault();

            var worker = (
                from s in _context.services
                join u in _context.workers
                    on s.worker_id equals u.worker_id
                where s.service_id == job.service_id
                select u
            ).FirstOrDefault();


            if (worker != null)
            {
                job.worker_id = worker.worker_id;
            }


            if (dto.image_1 != null && dto.image_1.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/jobs/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}.webp";
                var fullPath = Path.Combine(folder, fileName);

                await _imageService.SaveOptimizedImageAsync(dto.image_1, fullPath);

                job.image_1_url = "/img/jobs/" + fileName;
            }

            if (dto.image_2 != null && dto.image_2.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/jobs/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}.webp";
                var fullPath = Path.Combine(folder, fileName);

                await _imageService.SaveOptimizedImageAsync(dto.image_2, fullPath);

                job.image_2_url = "/img/jobs/" + fileName;
            }
          
            await _context.jobs.AddAsync(job);

            var payment = new Payment
            {
                job_id = job.job_id,
                labor_cost = service.labor_price,
                km_cost = worker.km_cost,
                payment_method = dto.payment_method,
                payment_status = "pending"
            };

            await _context.payment.AddAsync(payment);

            _ = Task.Run(() => _fcmService.SendNotificationByWorker(
                  job.worker_id,
                  "Servcio Nuevo",
                  "Tienes un nuevo servicio, verificalo", "Job"
              ));


            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            var jobs = await _context.jobs.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return jobs;
        }

        public async Task<object?> GetReviewJobByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from t in _context.jobs
                where t.is_active == true && t.job_id == id
                select new
                {
                    t.job_id,
                    t.client_id,

                    Worker = (
                        from s in _context.services
                        join w in _context.workers on s.worker_id equals w.worker_id
                        join u in _context.users on w.user_id equals u.user_id
                        where s.service_id == t.service_id
                        select new
                        {
                            u.user_id,
                            u.first_name,
                            u.username,
                            w.rating,
                            w.description,
                            w.city,
                            w.km_cost,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),
                    payment = _context.payment
                        .Where(c => c.job_id == t.job_id)
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
                    location = _context.locations
                        .Where(l => l.location_id == t.location_id)
                        .Select(l => new
                        {
                            l.location_id,
                            l.street,
                            l.neighborhood,
                            l.house_number,
                            l.state,
                            l.maps_address,
                            l.reference,
                            l.longitude,
                            l.latitude,
                            Image = string.IsNullOrEmpty(l.image_url)
                                ? null
                                : baseUrl + l.image_url,
                        }).FirstOrDefault(),

                    service = _context.services
                        .Where(s => s.service_id == t.service_id)
                        .Select(s => new
                        {
                            s.service_id,
                            s.service_name,
                            s.description,
                            s.labor_price,
                            s.duration_hours,
                            Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                        }).FirstOrDefault(),

                    payment = _context.payment
                        .Where(c => c.job_id == t.job_id)
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

                    Evidence = _context.evidence
                        .Where(e => e.job_id == t.job_id)
                        .Select(e => baseUrl + e.image_url)
                        .ToList(),
                    Review = _context.reviews
                        .Any(f =>
                            f.job_id == t.job_id  &&
                            f.is_active == true
                        ),
                    t.job_date,
                    t.job_time,
                    t.description,
                    t.problem,
                    t.is_active,
                    t.job_status,
                    t.description_worker,

                    Image_1 = string.IsNullOrEmpty(t.image_1_url)
                        ? null
                        : baseUrl + t.image_1_url,

                    Image_2 = string.IsNullOrEmpty(t.image_2_url)
                        ? null
                        : baseUrl + t.image_2_url,
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetReviewJobByIdWorkerAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from t in _context.jobs
                where t.is_active == true && t.job_id == id
                select new
                {
                    t.job_id,
                    t.worker_id,

                    Client = (
                        from u in _context.users
                        where u.user_id == t.client_id
                        select new
                        {
                            u.first_name,
                            u.username,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),

                    location = _context.locations
                        .Where(l => l.location_id == t.location_id)
                        .Select(l => new
                        {
                            l.location_id,
                            l.street,
                            l.neighborhood,
                            l.house_number,
                            l.state,
                            l.maps_address,
                            l.reference,
                            l.longitude,
                            l.latitude,
                            Image = string.IsNullOrEmpty(l.image_url)
                                ? null
                                : baseUrl + l.image_url,
                        }).FirstOrDefault(),

                    service = _context.services
                        .Where(s => s.service_id == t.service_id)
                        .Select(s => new
                        {
                            s.service_id,
                            s.service_name,
                            s.description,
                            s.labor_price,
                            s.duration_hours,
                            Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                        }).FirstOrDefault(),

                    payment = _context.payment
                        .Where(c => c.job_id == t.job_id)
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

                    Evidence = _context.evidence
                        .Where(e => e.job_id == t.job_id)
                        .Select(e => baseUrl + e.image_url)
                        .ToList(),

                    t.job_date,
                    t.job_time,
                    t.description,
                    t.problem,
                    t.is_active,
                    t.job_status,
                    t.description_worker,

                    Image_1 = string.IsNullOrEmpty(t.image_1_url)
                        ? null
                        : baseUrl + t.image_1_url,

                    Image_2 = string.IsNullOrEmpty(t.image_2_url)
                        ? null
                        : baseUrl + t.image_2_url,
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetJobsByUserIdAsync(Guid userId)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from t in _context.jobs
                where t.is_active == true && t.client_id == userId
                select new
                {
                    t.job_id,

                    Worker = (
                        from s in _context.services
                        join w in _context.workers on s.worker_id equals w.worker_id
                        join u in _context.users on w.user_id equals u.user_id
                        where s.service_id == t.service_id
                        select new
                        {
                            u.user_id,
                            u.first_name,
                            u.username,
                            Image = string.IsNullOrEmpty(u.image_url)
                                ? null
                                : baseUrl + u.image_url
                        }
                    ).FirstOrDefault(),

                    t.client_id,

                    location = _context.locations
                        .Where(l => l.location_id == t.location_id)
                        .Select(l => new
                        {
                            l.maps_address,
                        }).FirstOrDefault(),

                    service = _context.services
                        .Where(s => s.service_id == t.service_id)
                        .Select(s => new
                        {
                            s.service_id,
                            s.service_name,
                            s.description,
                            Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                        }).FirstOrDefault(),
                    payment = _context.payment
                        .Where(c => c.job_id == t.job_id)
                        .Select(c => new
                        {
                           
                            c.labor_cost,
                           
                        }).FirstOrDefault(),

                    t.job_date,
                    t.job_time,
                    t.description,
                    t.problem,
                    t.is_active,
                    t.job_status,
                }
            ).ToListAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task<object?> GetJobsByWorkerIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var result = await (
                from t in _context.jobs
                join w in _context.workers on t.worker_id equals w.worker_id
                join u in _context.users on w.user_id equals u.user_id
                where t.is_active == true && w.user_id == id
                select new
                {
                    t.job_id,

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

                    location = _context.locations
                        .Where(l => l.location_id == t.location_id)
                        .Select(l => new
                        {
                            l.maps_address,
                        }).FirstOrDefault(),

                    service = _context.services
                        .Where(s => s.service_id == t.service_id)
                        .Select(s => new
                        {
                            s.service_id,
                            s.service_name,
                            s.description,
                            Image = string.IsNullOrEmpty(s.image_url)
                                ? null
                                : baseUrl + s.image_url
                        }).FirstOrDefault(),

                    t.job_date,
                    t.job_time,
                    t.description,
                    t.problem,
                    t.is_active,
                    payment = _context.payment
                        .Where(c => c.job_id == t.job_id)
                        .Select(c => new
                        {
                           
                            c.labor_cost,
                    
                        }).FirstOrDefault(),
                    t.job_status,
                }
            ).ToListAsync();

            if (result == null)
                return null;

            return result;
        }

        public async Task DeleteJobAsync(Guid id)
        {
            var existing = await _context.express.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            var user = await _context.users.FindAsync(existing.client_id);
            if (existing != null)
            {

                _context.express.Remove(existing);
                if (!string.IsNullOrEmpty(existing.image_url))
                {
                    var imagePath = existing.image_url.TrimStart('/');

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

                if (existing.job_status != "pending")
                    await _fcmService.SendNotificationByWorker(
                        existing.worker_id.Value,
                         "Servicio Express Cancelado",
                         $"{user.username} ha cancelado el servicio express que habías aceptado.",
                         "cancelation_express"
                     );
            }
        }
        
        public async Task CancelJobAsync(Guid id)
        {
            var existing = await _context.jobs.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            var user = await _context.users.FindAsync(existing.client_id);

            existing.job_status = "canceled";
            if (existing.job_status != "pending")
            await _fcmService.SendNotificationByWorker(
                existing.worker_id,
                    "Trabajo agendado Cancelado",
                    $"{user.username} ha cancelado el trabajado agendado.",
                    "cancelation_job"
                );

            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobStatusAsync(Guid id, string action)
        {

            var existing = await _context.jobs.FindAsync(id);
            

            if (existing == null)
                throw new Exception("Trabajo no encontrado");
            
            var service = await _context.services.FindAsync(existing.service_id);
            switch (action)
            {
                case "pending":

                    existing.job_status = "accepted";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio aceptado ",
                        $"El servicio '{service.service_name}' fue aceptado correctamente.", "Job"
                    );

                    break;
                case "accepted":

                    var now = DateTime.Now;

                    // Convertir DateOnly + TimeOnly → DateTime
                    var jobDateTime = existing.job_date.ToDateTime(existing.job_time);

                    // Validar: mismo día y máximo 30 minutos antes
                    if (now < jobDateTime.AddMinutes(-90) || now.Date != jobDateTime.Date)
                        throw new Exception("Solo puedes iniciar el servicio el mismo día o 30 minutos antes de la hora programada.");

                    existing.job_status = "going";
                    existing.start_job = DateTime.UtcNow;

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "El trabajador esta llendo ",
                        $"El trabajador de '{service.service_name}' esta llendo a tu domicilio.", "Job"
                    );

                    break;
                case "going":

                    existing.job_status = "arrived";
                    
                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "El trabajador ya llego",
                        $"El trabajador de '{service.service_name}' ya llego a tu domicilio.", "Job"
                    );

                    break;
                case "arrived":
                    existing.job_status = "diagnosing";

                    await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador ya diagnosito ",
                            $"El trabajador de '{existing.problem}' ya diagnositico tu problema.", "Job"
                    );
                    

                    break;

                case "in_progress":

                    existing.job_status = "finalized";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio finalizado 🎉",
                        $"El servicio '{service.service_name}' ha sido completado.","Job"
                    );

                    break;

                case "canceled":

                    existing.job_status = "canceled";

                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio cancelado ❌",
                        $"El servicio '{service.service_name}' fue cancelado por el trabajador.", "Job"
                    );

                    break;

                default:
                    throw new Exception("Acción no válida");
            }

            await _context.SaveChangesAsync();
        }

        public async Task AceptDiagnosticAsync(Guid id)
        {
            var existing = await _context.jobs.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            var service = await _context.services.FindAsync(existing.service_id);

            existing.job_status = "in_progress";

            await _fcmService.SendNotificationByWorker(
                  existing.worker_id,
                  "¡Diagnóstico aprobado!",
                  $"El cliente aceptó tu diagnóstico para '{service.service_name}'. Puedes comenzar el servicio.",
                  "Job"
             );

            await _context.SaveChangesAsync();
        }

        public async Task FinishDiagnosticAsync(Guid id)
        {
            var existing = await _context.jobs.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");

            var service = await _context.services.FindAsync(existing.service_id);

            existing.job_status = "completed";

            await _fcmService.SendNotificationByWorker(
                existing.worker_id,
                "Diagnóstico rechazado",
                $"El cliente no aceptó el diagnóstico de '{service.service_name}'. No te preocupes, se te pagará la visita de diagnóstico.",
                "Job"
            );

            await _context.SaveChangesAsync();
        }

        public async Task FinishJobAsync(Guid id)
        {
            var existing = await _context.jobs.FindAsync(id);

            if (existing == null)
                throw new Exception("Trabajo no encontrado");


            existing.job_status = "completed";
            existing.finish_job = DateTime.UtcNow;


            await _fcmService.SendNotificationByWorker(
                 existing.worker_id,
                 "¡Trabajo completado!",
                 $"Has finalizado exitosamente el servicio '{existing.problem}'. ¡Buen trabajo!",
                 "Job"
             );

            await _fcmService.SendNotificationByUser(
                existing.client_id,
                " ¡Servicio completado!",
                $"Tu servicio '{existing.problem}' ha sido finalizado con éxito. ¡Gracias por confiar en nosotros!",
                "Job"
            );

            await _context.SaveChangesAsync();
        }

    }

}
