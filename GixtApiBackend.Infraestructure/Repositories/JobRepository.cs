using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.IO;
using System.Threading.Tasks;

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
            try
            {
                // Construimos el trabajo con los datos recibidos del DTO
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

                // El trabajo inicia en estado pendiente y activo
                job.job_status = "pending";
                job.is_active = true;

                // Buscamos el servicio para obtener su precio de mano de obra
                var service = _context.services
                    .Where(s => s.service_id == job.service_id)
                    .FirstOrDefault();

                // Buscamos el trabajador asociado a ese servicio
                var worker = (
                    from s in _context.services
                    join u in _context.workers
                        on s.worker_id equals u.worker_id
                    where s.service_id == job.service_id
                    select u
                ).FirstOrDefault();

                // Si hay trabajador, lo asignamos al trabajo
                if (worker != null)
                {
                    job.worker_id = worker.worker_id;
                }

                // Si llega la primera imagen, la optimizamos y guardamos
                if (dto.image_1 != null && dto.image_1.Length > 0)
                {

                    var img = await _imageService.SaveImageAsync(dto.image_1, "jobs");
                    job.image_1_url = img;
                }

                // Si llega la segunda imagen, la optimizamos y guardamos
                if (dto.image_2 != null && dto.image_2.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.image_2, "jobs");
                    job.image_2_url = img;
                }

                // Registramos el trabajo en el contexto
                await _context.jobs.AddAsync(job);

                // Creamos el pago asociado con los costos del servicio y del trabajador
                var payment = new Payment
                {
                    job_id = job.job_id,
                    labor_cost = service.labor_price,
                    km_cost = worker.km_cost,
                    payment_method = dto.payment_method,
                    payment_status = "pending"
                };

                await _context.payment.AddAsync(payment);

                // Enviamos la notificación al trabajador en segundo plano para no bloquear la respuesta
                _ = Task.Run(() => _fcmService.SendNotificationByWorker(
                    job.worker_id,
                    "Servcio Nuevo",
                    "Tienes un nuevo servicio, verificalo", "Job"
                ));

                // Guardamos trabajo y pago en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir el trabajo o el pago en la base de datos
                throw new Exception("Error creating the job in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            try
            {
                // Obtenemos todos los trabajos de la base de datos
                var jobs = await _context.jobs.ToListAsync();

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Devolvemos la lista de trabajos
                return jobs;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los trabajos
                throw new Exception("Error retrieving the jobs list", ex);
            }
        }

        public async Task<object?> GetReviewJobByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el trabajo activo con todos sus datos relacionados (vista del cliente)
                var result = await (
                    from t in _context.jobs
                    where t.is_active == true && t.job_id == id
                    select new
                    {
                        t.job_id,
                        t.client_id,
                        // Datos del trabajador que ofrece el servicio
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
                        // Datos de la ubicación del trabajo
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
                        // Datos del servicio contratado
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
                        // Información del pago asociado al trabajo
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
                        // Lista de URLs de las evidencias del trabajo
                        Evidence = _context.evidence
                            .Where(e => e.job_id == t.job_id)
                            .Select(e => baseUrl + e.image_url)
                            .ToList(),
                        // Indica si el trabajo ya tiene una reseña activa
                        Review = _context.reviews
                            .Any(f =>
                                f.job_id == t.job_id &&
                                f.is_active == true
                            ),
                        t.job_date,
                        t.job_time,
                        t.description,
                        t.problem,
                        t.is_active,
                        t.job_status,
                        t.description_worker,
                        // URLs de las imágenes del trabajo
                        Image_1 = string.IsNullOrEmpty(t.image_1_url)
                            ? null
                            : baseUrl + t.image_1_url,
                        Image_2 = string.IsNullOrEmpty(t.image_2_url)
                            ? null
                            : baseUrl + t.image_2_url,
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el trabajo, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el trabajo por id
                throw new Exception("Error retrieving the job review by id", ex);
            }
        }

        public async Task<object?> GetReviewJobByIdWorkerAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el trabajo activo con sus datos relacionados (vista del trabajador)
                var result = await (
                    from t in _context.jobs
                    where t.is_active == true && t.job_id == id
                    select new
                    {
                        t.job_id,
                        t.worker_id,
                        // Datos básicos del cliente
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
                        // Datos de la ubicación del trabajo
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
                        // Datos del servicio contratado
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
                        // Información del pago asociado al trabajo
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
                        // Lista de URLs de las evidencias del trabajo
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
                        // URLs de las imágenes del trabajo
                        Image_1 = string.IsNullOrEmpty(t.image_1_url)
                            ? null
                            : baseUrl + t.image_1_url,
                        Image_2 = string.IsNullOrEmpty(t.image_2_url)
                            ? null
                            : baseUrl + t.image_2_url,
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el trabajo, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el trabajo por id (vista trabajador)
                throw new Exception("Error retrieving the job review by id (worker view)", ex);
            }
        }

        public async Task<object?> GetJobsByUserIdAsync(Guid userId)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos los trabajos activos del cliente indicado
                var result = await (
                    from t in _context.jobs
                    where t.is_active == true && t.client_id == userId
                    select new
                    {
                        t.job_id,
                        // Datos del trabajador asignado al servicio
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
                        // Solo necesitamos la dirección de la ubicación
                        location = _context.locations
                            .Where(l => l.location_id == t.location_id)
                            .Select(l => new
                            {
                                l.maps_address,
                            }).FirstOrDefault(),
                        // Datos básicos del servicio
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
                        // Solo el costo de mano de obra del pago
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

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de trabajos
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los trabajos del cliente
                throw new Exception("Error retrieving the jobs by user", ex);
            }
        }

        public async Task<object?> GetJobsByWorkerIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos los trabajos activos asignados al trabajador indicado
                var result = await (
                    from t in _context.jobs
                    join w in _context.workers on t.worker_id equals w.worker_id
                    join u in _context.users on w.user_id equals u.user_id
                    where t.is_active == true && w.user_id == id
                    select new
                    {
                        t.job_id,
                        // Datos básicos del cliente
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
                        // Solo necesitamos la dirección de la ubicación
                        location = _context.locations
                            .Where(l => l.location_id == t.location_id)
                            .Select(l => new
                            {
                                l.maps_address,
                            }).FirstOrDefault(),
                        // Datos básicos del servicio
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
                        // Solo el costo de mano de obra del pago
                        payment = _context.payment
                            .Where(c => c.job_id == t.job_id)
                            .Select(c => new
                            {
                                c.labor_cost,
                            }).FirstOrDefault(),
                        t.job_status,
                    }
                ).ToListAsync();

                // Si no hay resultados, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de trabajos
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los trabajos del trabajador
                throw new Exception("Error retrieving the jobs by worker", ex);
            }
        }

        public async Task DeleteJobAsync(Guid id)
        {
            try
            {
                // Buscamos el registro por su id
                var existing = await _context.express.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Obtenemos el usuario cliente para usar su nombre en la notificación
                var user = await _context.users.FindAsync(existing.client_id);

                if (existing != null)
                {
                    // Eliminamos el registro del contexto
                    _context.express.Remove(existing);

                    // Si tiene imagen, la eliminamos físicamente del servidor
                    if (!string.IsNullOrEmpty(existing.image_url))
                    {
                        _imageService.DeleteImageAsync(existing.image_url);
                    }

                    // Guardamos los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    // Si el trabajo ya no estaba pendiente, notificamos al trabajador de la cancelación
                    if (existing.job_status != "pending")
                        await _fcmService.SendNotificationByWorker(
                            existing.worker_id.Value,
                            "Servicio Express Cancelado",
                            $"{user.username} ha cancelado el servicio express que habías aceptado.",
                            "cancelation_express"
                        );
                }
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task CancelJobAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo por su id
                var existing = await _context.jobs.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Obtenemos el usuario cliente para usar su nombre en la notificación
                var user = await _context.users.FindAsync(existing.client_id);

                // Cambiamos el estado del trabajo a cancelado
                existing.job_status = "canceled";

                // Si el trabajo no estaba pendiente, notificamos al trabajador
                if (existing.job_status != "pending")
                    await _fcmService.SendNotificationByWorker(
                        existing.worker_id,
                        "Trabajo agendado Cancelado",
                        $"{user.username} ha cancelado el trabajado agendado.",
                        "cancelation_job"
                    );

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task UpdateJobStatusAsync(Guid id, string action)
        {
            try
            {
                // Buscamos el trabajo por su id
                var existing = await _context.jobs.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Obtenemos el servicio para usar su nombre en las notificaciones
                var service = await _context.services.FindAsync(existing.service_id);

                // Avanzamos el estado del trabajo según el estado actual recibido
                switch (action)
                {
                    // De pendiente pasa a aceptado
                    case "pending":
                        existing.job_status = "accepted";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "Servicio aceptado ",
                            $"El servicio '{service.service_name}' fue aceptado correctamente.", "Job"
                        );
                        break;

                    // De aceptado pasa a "en camino", validando la ventana de tiempo
                    case "accepted":
                        var now = DateTime.Now;

                        // Convertimos DateOnly + TimeOnly a DateTime
                        var jobDateTime = existing.job_date.ToDateTime(existing.job_time);

                        // Validamos: mismo día y como máximo 90 minutos antes de la hora programada
                        if (now < jobDateTime.AddMinutes(-90) || now.Date != jobDateTime.Date)
                            throw new InvalidOperationException("You can only start the service on the same day or shortly before the scheduled time.");

                        existing.job_status = "going";
                        existing.start_job = DateTime.UtcNow;

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador esta llendo ",
                            $"El trabajador de '{service.service_name}' esta llendo a tu domicilio.", "Job"
                        );
                        break;

                    // De "en camino" pasa a "llegó"
                    case "going":
                        existing.job_status = "arrived";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador ya llego",
                            $"El trabajador de '{service.service_name}' ya llego a tu domicilio.", "Job"
                        );
                        break;

                    // De "llegó" pasa a "diagnosticando"
                    case "arrived":
                        existing.job_status = "diagnosing";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador ya diagnosito ",
                            $"El trabajador de '{existing.problem}' ya diagnositico tu problema.", "Job"
                        );
                        break;

                    // De "en progreso" pasa a finalizado
                    case "in_progress":
                        existing.job_status = "finalized";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "Servicio finalizado 🎉",
                            $"El servicio '{service.service_name}' ha sido completado.", "Job"
                        );
                        break;

                    // El trabajo se cancela
                    case "canceled":
                        existing.job_status = "canceled";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "Servicio cancelado ❌",
                            $"El servicio '{service.service_name}' fue cancelado por el trabajador.", "Job"
                        );
                        break;

                    // Cualquier otra acción no es válida
                    default:
                        throw new ArgumentException("Invalid action");
                }

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task AceptDiagnosticAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo por su id
                var existing = await _context.jobs.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Obtenemos el servicio para usar su nombre en la notificación
                var service = await _context.services.FindAsync(existing.service_id);

                // El cliente aceptó el diagnóstico, el trabajo pasa a "en progreso"
                existing.job_status = "in_progress";

                // Notificamos al trabajador que su diagnóstico fue aprobado
                await _fcmService.SendNotificationByWorker(
                    existing.worker_id,
                    "¡Diagnóstico aprobado!",
                    $"El cliente aceptó tu diagnóstico para '{service.service_name}'. Puedes comenzar el servicio.",
                    "Job"
                );

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task FinishDiagnosticAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo por su id
                var existing = await _context.jobs.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Obtenemos el servicio para usar su nombre en la notificación
                var service = await _context.services.FindAsync(existing.service_id);

                // El cliente rechazó el diagnóstico, el trabajo se marca como completado
                existing.job_status = "completed";

                // Notificamos al trabajador que el diagnóstico fue rechazado (se le paga la visita)
                await _fcmService.SendNotificationByWorker(
                    existing.worker_id,
                    "Diagnóstico rechazado",
                    $"El cliente no aceptó el diagnóstico de '{service.service_name}'. No te preocupes, se te pagará la visita de diagnóstico.",
                    "Job"
                );

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task FinishJobAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo por su id
                var existing = await _context.jobs.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Marcamos el trabajo como completado y registramos la hora de finalización
                existing.job_status = "completed";
                existing.finish_job = DateTime.UtcNow;

                // Notificamos al trabajador que completó el trabajo
                await _fcmService.SendNotificationByWorker(
                    existing.worker_id,
                    "¡Trabajo completado!",
                    $"Has finalizado exitosamente el servicio '{existing.problem}'. ¡Buen trabajo!",
                    "Job"
                );

                // Notificamos al cliente que su servicio fue finalizado
                await _fcmService.SendNotificationByUser(
                    existing.client_id,
                    " ¡Servicio completado!",
                    $"Tu servicio '{existing.problem}' ha sido finalizado con éxito. ¡Gracias por confiar en nosotros!",
                    "Job"
                );

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }
    }
}