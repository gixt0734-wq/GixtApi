using FirebaseAdmin.Messaging;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.IO;
using System.Threading.Tasks;


namespace GixtApiBackend.Infraestructure.Repositories
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
            try
            {
                // Construimos la entidad express con los datos recibidos del DTO
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

                // El trabajo inicia activo y en estado pendiente
                job.is_active = true;
                job.job_status = "pending";

                // Si llega una imagen, la guardamos y asignamos su URL al trabajo
                if (dto.image != null && dto.image.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.image, "jobs_express");
                    job.image_url = img;
                }

                // Registramos el trabajo express en el contexto
                await _context.express.AddAsync(job);

                // Creamos el pago asociado al trabajo, en estado pendiente
                var payment = new Payment
                {
                    job_id = job.express_id,
                    payment_method = dto.payment_method,
                    payment_status = "pending"
                };

                // Registramos el pago y guardamos ambos cambios en la base de datos
                await _context.payment.AddAsync(payment);
                await _context.SaveChangesAsync();

                // Notificamos a los trabajadores de la categoría que hay un nuevo servicio express
                await _fcmService.SendNotificationByExpress(
                    job.category_id,
                    "Alguien necesita tu ayuda",
                    "Tienes un nuevo servicio express, verificalo",
                    job.express_id,
                    job.client_id
                );

                // Devolvemos el id del trabajo creado
                return job.express_id;
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir el trabajo o el pago en la base de datos
                throw new Exception("Error creating the express job in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task SendAlertExpress(Guid user_id)
        {
            try
            {
                // Buscamos un trabajo express pendiente y activo del cliente indicado
                var existing = await _context.express
                    .FirstOrDefaultAsync(p =>
                        p.client_id == user_id &&
                        p.job_status == "pending" &&
                        p.is_active == true
                    );

                // Si no existe el trabajo, no podemos enviar la alerta
                if (existing == null)
                {
                    throw new Exception("Job not found");
                }

                // Reenviamos la notificación a los trabajadores de la categoría
                await _fcmService.SendNotificationByExpress(
                    existing.category_id,
                    "Alguien necesita tu ayuda",
                    "Tienes un nuevo servicio express, verificalo",
                    existing.express_id,
                    existing.client_id
                );
            }
            catch (Exception)
            {
                // Relanzamos el error para que lo maneje la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Express>> GetAllExpresssAsync()
        {
            try
            {
                // Obtenemos todos los trabajos express de la base de datos
                var jobs = await _context.express.ToListAsync();

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Devolvemos la lista de trabajos
                return jobs;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los trabajos express
                throw new Exception("Error retrieving the express jobs list", ex);
            }
        }

        public async Task<object> GetExpressReviewIdAsync(Guid id, Guid idworker)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el trabajo express activo con todos sus datos relacionados
                var result = await (
                    from e in _context.express
                    where e.is_active == true && e.express_id == id
                    select new
                    {
                        e.express_id,
                        e.client_id,
                        e.worker_id,
                        // Datos básicos del cliente
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
                        // Datos del trabajador que consulta
                        Worker = (
                            from w in _context.workers
                            where w.user_id == idworker
                            select new
                            {
                                w.km_cost,
                                w.user_id
                            }
                        ).FirstOrDefault(),
                        // Información del pago asociado al trabajo
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
                        // Lista de URLs de las evidencias del trabajo
                        Evidence = _context.evidence
                            .Where(c => c.job_id == e.express_id)
                            .Select(c => baseUrl + c.image_url)
                            .ToList(),
                        // Indica si el trabajo ya tiene una reseña activa
                        Review = _context.reviews
                            .Any(f =>
                                f.job_id == e.express_id &&
                                f.is_active == true
                            ),
                        e.job_date,
                        e.job_time,
                        e.latitude,
                        e.longitude,
                        e.maps_address,
                        e.description,
                        e.description_worker,
                        e.problem,
                        e.is_active,
                        e.job_status,
                        // URL de la imagen del trabajo
                        Image = string.IsNullOrEmpty(e.image_url)
                            ? null
                            : baseUrl + e.image_url,
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el trabajo, devolvemos null
                if (result == null)
                    return null;

                // Si el trabajo ya no está pendiente, validamos que el trabajador tenga permiso
                if (result.job_status != "pending")
                {
                    var worker = await _context.workers.FirstOrDefaultAsync(w => w.user_id == idworker);

                    // Validamos que el trabajador exista antes de comparar (evita NullReferenceException)
                    if (worker == null)
                        throw new InvalidOperationException("Worker not found");

                    // Solo el trabajador asignado puede consultar este trabajo
                    if (result.worker_id != worker.worker_id)
                        throw new UnauthorizedAccessException("You do not have permission to access this job.");
                }

                // Devolvemos el resultado
                return result;
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task<object?> GetExpressByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el trabajo express activo con sus datos relacionados
                var result = await (
                    from e in _context.express
                    where e.is_active == true && e.express_id == id
                    select new
                    {
                        e.express_id,
                        e.client_id,
                        // Datos del trabajador asignado (con join a users)
                        Worker = (
                            from s in _context.express
                            join w in _context.workers on s.worker_id equals w.worker_id
                            join u in _context.users on w.user_id equals u.user_id
                            where s.express_id == e.express_id
                            select new
                            {
                                u.user_id,
                                u.first_name,
                                u.username,
                                w.rating,
                                w.description,
                                Image = string.IsNullOrEmpty(u.image_url)
                                    ? null
                                    : baseUrl + u.image_url
                            }
                        ).FirstOrDefault(),
                        // Información del pago asociado al trabajo
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
                        // Lista de URLs de las evidencias del trabajo
                        Evidence = _context.evidence
                            .Where(c => c.job_id == e.express_id)
                            .Select(c => baseUrl + c.image_url)
                            .ToList(),
                        // Indica si el trabajo ya tiene una reseña activa
                        Review = _context.reviews
                            .Any(f =>
                                f.job_id == e.express_id &&
                                f.is_active == true
                            ),
                        e.job_date,
                        e.job_time,
                        e.latitude,
                        e.longitude,
                        e.maps_address,
                        e.description_worker,
                        e.description,
                        e.problem,
                        e.is_active,
                        e.job_status,
                        // URL de la imagen del trabajo
                        Image = string.IsNullOrEmpty(e.image_url)
                            ? null
                            : baseUrl + e.image_url,
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el trabajo, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
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
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos los trabajos express activos asignados a este trabajador
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
                        // URL de la imagen del trabajo
                        Image = string.IsNullOrEmpty(t.image_url)
                            ? null
                            : baseUrl + t.image_url,
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
                        t.job_date,
                        t.job_time,
                        t.is_active,
                        t.job_status,
                        t.maps_address
                    }
                ).ToListAsync();

                // Si no se encontraron trabajos, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos la lista de trabajos
                return result;
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task DeleteExpressAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var job = await _context.express.FindAsync(id);

                // Buscamos el pago asociado al trabajo
                var payment = _context.payment
                    .Where(p => p.job_id == job.express_id)
                    .FirstOrDefault();

                // Si el trabajo no existe, no hay nada que eliminar
                if (job == null)
                {
                    return;
                }

                // Obtenemos el usuario cliente para usar su nombre en la notificación
                var user = await _context.users.FindAsync(job.client_id);

                if (job != null)
                {
                    // Eliminamos el trabajo y su pago del contexto
                    _context.express.Remove(job);
                    _context.payment.Remove(payment);

                    // Si el trabajo tiene imagen, la eliminamos físicamente del servidor
                    if (!string.IsNullOrEmpty(job.image_url))
                    {
                        _imageService.DeleteImageAsync(job.image_url);

                    }

                    // Guardamos los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    // Si el trabajo ya no estaba pendiente, notificamos al trabajador de la cancelación
                    if (job.job_status != "pending")
                        await _fcmService.SendNotificationByWorker(
                            job.worker_id.Value,
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

        public async Task CancelExpressAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var existing = await _context.express.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Cambiamos el estado del trabajo a aceptado
                existing.job_status = "accepted";

                // Obtenemos datos del trabajador asignado
                var workerData = await (
                    from u in _context.users
                    join w in _context.workers on u.user_id equals w.user_id
                    where u.user_id == existing.worker_id
                    select new
                    {
                        Username = u.username,
                        WorkerId = w.worker_id
                    }).FirstOrDefaultAsync();

                // Si no hay trabajador asociado, terminamos sin notificar
                if (workerData == null)
                    return;

                // Notificamos al cliente que el trabajador aceptó
                await _fcmService.SendNotificationByUser(
                    existing.client_id,
                    "El cliente a aceptado",
                    $"El servicio express '{existing.problem}' fue accepto tu solicitud, ve rapido.", "Express"
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

        public async Task SendAcceptAsync(Guid worker, Guid id, decimal km_cost, decimal labor_price)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var service = await _context.express.FindAsync(id);

                // Si no existe, terminamos sin hacer nada
                if (service == null)
                    return;

                // Si el trabajo ya tiene trabajador asignado, no se puede proponer
                if (service.worker_id != null)
                    throw new Exception("This job was already taken by another worker.");

                // Obtenemos datos del trabajador que envía la propuesta
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

                // Si el trabajador no existe, terminamos
                if (workerData == null)
                    return;

                // Enviamos la propuesta del trabajador al cliente vía notificación
                await _fcmService.sendNotificationByExpress(
                    id,
                    workerData.Username,
                    workerData.WorkerId,
                    $"{workerData.Username} a enviado una propuesta.",
                    $"{workerData.Username} realiza el trabajo por: {labor_price} y por diagnostico ${km_cost}",
                    labor_price, km_cost
                );
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task AcceptExpressAsync(Guid express_id, Guid worker_id, decimal km_cost, decimal labor_price)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var existing = await _context.express.FindAsync(express_id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new Exception("Job not found");

                // Evitamos aceptar dos veces el mismo trabajo
                if (existing.job_status == "accepted")
                    throw new Exception("You already accepted this job previously.");

                // Asignamos el trabajo al trabajador y cambiamos su estado
                existing.job_status = "accepted";
                existing.worker_id = worker_id;

                // Buscamos el pago asociado para registrar los costos
                var existingpay = _context.payment
                    .Where(p => p.job_id == express_id)
                    .FirstOrDefault();

                // Si existe el pago, actualizamos costos de kilometraje y mano de obra
                if (existingpay != null)
                {
                    existingpay.km_cost = km_cost;
                    existingpay.labor_cost = labor_price;
                }

                // Notificamos al trabajador que el cliente aceptó
                await _fcmService.SendNotificationByWorker(
                    worker_id,
                    "El cliente a aceptado",
                    $"El servicio express '{existing.problem}' fue accepto tu solicitud, ve rapido.", "Express"
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

        public async Task UpdateExpressStatusAsync(Guid id, string action)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var existing = await _context.express.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new Exception("Job not found");

                // Avanzamos el estado del trabajo según la acción/estado actual recibido
                switch (action)
                {
                    // De pendiente pasa a aceptado
                    case "pending":
                        existing.job_status = "accepted";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "Servicio aceptado ",
                            $"El servicio '{existing.problem}' fue aceptado correctamente.", "Express"
                        );
                        break;

                    // De aceptado pasa a "en camino" y registramos la hora de inicio
                    case "accepted":
                        existing.job_status = "going";
                        existing.start_job = DateTime.UtcNow;

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador esta llendo ",
                            $"El trabajador de '{existing.problem}' esta llendo a tu domicilio.", "Express"
                        );
                        break;

                    // De "en camino" pasa a "llegó"
                    case "going":
                        existing.job_status = "arrived";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "El trabajador ya llego",
                            $"El trabajador de '{existing.problem}' ya llego a tu domicilio.", "Express"
                        );
                        break;

                    // El trabajo se cancela
                    case "canceled":
                        existing.job_status = "canceled";

                        await _fcmService.SendNotificationByUser(
                            existing.client_id,
                            "Servicio cancelado ❌",
                            $"El servicio '{existing.problem}' fue cancelado por el trabajador.", "Express"
                        );
                        break;

                    // Cualquier otra acción no es válida
                    default:
                        throw new Exception("Invalid action");
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

        public async Task AceptDiagnosticExpAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var existing = await _context.express.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // El cliente aceptó el diagnóstico, el trabajo pasa a "en progreso"
                existing.job_status = "in_progress";

                // Validamos que haya un trabajador asignado
                if (existing.worker_id == null)
                {
                    throw new InvalidOperationException("Worker not found");
                }

                // Notificamos al trabajador que su diagnóstico fue aprobado
                await _fcmService.SendNotificationByWorker(
                    existing.worker_id.Value,
                    "¡Diagnóstico aprobado!",
                    $"El cliente aceptó tu diagnóstico para '{existing.problem}'. Puedes comenzar el servicio.",
                    "Express"
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

        public async Task FinishExpressAsync(Guid id)
        {
            try
            {
                // Buscamos el trabajo express por su id
                var existing = await _context.express.FindAsync(id);

                // Si no existe, lanzamos error
                if (existing == null)
                    throw new InvalidOperationException("Job not found");

                // Marcamos el trabajo como completado y registramos la hora de finalización
                existing.job_status = "completed";
                existing.finish_job = DateTime.UtcNow;

                // Validamos que haya un trabajador asignado
                if (existing.worker_id == null)
                {
                    throw new InvalidOperationException("Worker not found");
                }

                // Notificamos al trabajador que completó el trabajo
                await _fcmService.SendNotificationByWorker(
                    existing.worker_id.Value,
                    "¡Trabajo completado!",
                    $"Has finalizado exitosamente el servicio '{existing.problem}'. ¡Buen trabajo!",
                    "Express"
                );

                // Notificamos al cliente que su servicio fue finalizado
                await _fcmService.SendNotificationByUser(
                    existing.client_id,
                    " ¡Servicio completado!",
                    $"Tu servicio '{existing.problem}' ha sido finalizado con éxito. ¡Gracias por confiar en nosotros!",
                    "Express"
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
