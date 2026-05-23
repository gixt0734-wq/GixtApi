using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;

namespace GixtApi.Infraestructure.Repositories
{
    public class WorkerRepository : IWorkerRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailService _emailService;
        private readonly ImageService _imageService;

        public WorkerRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, EmailService emailService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
            _emailService = emailService;
        }

        public async Task CreateWorkerAsync(UserDTO dto)
        {
            // Ruta para el posible rollback de la imagen si falla la BD
            string? fullPath = null;

            try
            {
                // Construimos el usuario con los datos recibidos del DTO
                var user = new User
                {
                    email = dto.email,
                    first_name = dto.first_name,
                    last_name = dto.last_name,
                    phone = dto.phone,
                    gender = dto.gender,
                    birth_date = dto.birth_date,
                    terms = dto.terms,
                };

                // Valores por defecto: rol trabajador, activo y contraseña encriptada
                user.rol_id = 3;
                user.is_active = true;
                user.password = BCrypt.Net.BCrypt.HashPassword(dto.password);

                // Verificamos que el correo no esté ya registrado
                var existingCliente = await _context.users.FirstOrDefaultAsync(c => c.email == dto.email);
                if (existingCliente != null)
                    throw new InvalidOperationException("Email is already registered.");

                // Generamos el username a partir del primer nombre y primer apellido
                string firstName = dto.first_name?.Split(' ')[0] ?? "";
                string lastName = dto.last_name?.Split(' ')[0] ?? "";
                string username = $"{firstName} {lastName}";
                user.username = username;

                // Si llega una imagen, la guardamos y asignamos su URL
                if (dto.imagen != null && dto.imagen.Length > 0)
                {
                    var img = await _imageService.SaveImageAsync(dto.imagen, "users");
                    user.image_url = img;
                }

                // Registramos el usuario y guardamos en la base de datos
                await _context.users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Plantilla HTML del correo de bienvenida para el trabajador
                string body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>¡Bienvenido!</title>
                </head>
                <body style="margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f4f4;">
                    <table role="presentation" style="width: 100%; border-collapse: collapse;">
                        <tr>
                            <td align="center" style="padding: 40px 0;">
                                <table role="presentation" style="width: 600px; max-width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.1);">
                                    <!-- Header -->
                                    <tr>
                                        <td style="padding: 0; background-color: #1E6AE1; border-radius: 8px 8px 0 0;">
                                            <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                                <tr>
                                                    <td style="padding: 50px 30px; text-align: center;">
                                                        <h1 style="margin: 0; color: #ffffff; font-size: 32px; font-weight: bold;">¡Bienvenido!</h1>
                                                        <p style="margin: 10px 0 0 0; color: #ffffff; font-size: 18px; opacity: 0.9;">Nos alegra que formes parte de nuestra comunidad</p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <!-- Body -->
                                    <tr>
                                        <td style="padding: 40px 30px;">
                                            <p style="margin: 0 0 20px 0; color: #333333; font-size: 18px; line-height: 26px;">
                                                Hola <strong>{username}</strong>,
                                            </p>
                                            <p style="margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 24px;">
                                                Gracias por unirte a nuestra comunidad. Tu cuenta ha sido creada exitosamente y ya puedes disfrutar de todos los beneficios que tenemos para ti.
                                            </p>
                                            <!-- Beneficios -->
                                            <table role="presentation" style="width: 100%; border-collapse: collapse; margin: 30px 0;">
                                                <tr>
                                                    <td style="padding: 20px; background-color: #f8f9fa; border-left: 4px solid #667eea; border-radius: 4px;">
                                                        <h3 style="margin: 0 0 15px 0; color: #333333; font-size: 18px;">¿Qué puedes hacer ahora?</h3>
                                                        <ul style="margin: 0; padding-left: 20px; color: #666666; font-size: 15px; line-height: 24px;">
                                                            <li style="margin-bottom: 10px;">Termina tu registro para resivir trabajos</li>
                                                            <li style="margin-bottom: 10px;">Explorar todas las funcionalidades disponibles</li>
                                                            <li style="margin-bottom: 10px;">Conectar con clientes</li>
                                                            <li style="margin-bottom: 0;">Explorar los diferentes servicios cercanos a ti</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                            <!-- Información adicional -->
                                            <table role="presentation" style="width: 100%; border-collapse: collapse; margin: 30px 0;">
                                                <tr>
                                                    <td style="padding: 20px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;">
                                                        <p style="margin: 0; color: #856404; font-size: 14px; line-height: 20px;">
                                                            <strong>Consejo:</strong> Empieza ya a explorar nuestra plataforma y descubre todo lo que puedes hacer.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                            <p style="margin: 30px 0 10px 0; color: #666666; font-size: 16px; line-height: 24px;">
                                                Si tienes alguna pregunta, no dudes en contactarnos. Estamos aquí para ayudarte.
                                            </p>
                                            <p style="margin: 0; color: #666666; font-size: 16px; line-height: 24px;">
                                                ¡Que tengas un excelente día!
                                            </p>
                                        </td>
                                    </tr>
                                    <!-- Separador -->
                                    <tr>
                                        <td style="padding: 0 30px;">
                                            <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                                <tr>
                                                    <td style="border-top: 1px solid #eeeeee;"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <!-- Footer -->
                                    <tr>
                                        <td style="padding: 30px;">
                                            <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                                <tr>
                                                    <td align="center">
                                                        <p style="margin: 0 0 15px 0; color: #333333; font-size: 14px; font-weight: bold;">
                                                            Síguenos en nuestras redes sociales
                                                        </p>
                                                        <table role="presentation" style="display: inline-block; border-collapse: collapse;">
                                                            <tr>
                                                                <td style="padding: 0 10px;"><a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Facebook</a></td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;"><a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Twitter</a></td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;"><a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Instagram</a></td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;"><a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">LinkedIn</a></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                            <table role="presentation" style="width: 100%; border-collapse: collapse; margin-top: 20px;">
                                                <tr>
                                                    <td align="center">
                                                        <p style="margin: 0; color: #999999; font-size: 12px; line-height: 18px;">
                                                            Has recibido este correo porque te registraste en la plataforma GIXT
                                                        </p>
                                                        <p style="margin: 10px 0 0 0; color: #999999; font-size: 12px;">
                                                            © 2026 GIXT. Todos los derechos reservados.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;

                // Enviamos el correo de bienvenida en segundo plano
                _ = Task.Run(async () => _emailService.SendEmailAsync(dto.email, body));
            }
            catch (Exception)
            {
                // Rollback de la imagen si falló la operación en la base de datos
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task CreateInfoWorkerAsync(WorkerDTO dto)
        {
            try
            {
                // Construimos la información del trabajador con los datos del DTO
                var worker = new Worker
                {
                    user_id = dto.user_id,
                    description = dto.description,
                    city = dto.city,
                    latitude = dto.latitude,
                    longitude = dto.longitude,
                    range_km = dto.range_km,
                    km_cost = dto.km_cost,
                };

                // Valores por defecto: activo, disponible para trabajar y sin calificación
                worker.is_active = true;
                worker.is_working = true;
                worker.rating = 0;

                // Verificamos que el trabajador no tenga ya su información registrada
                var existingCliente = await _context.workers.FirstOrDefaultAsync(w => w.user_id == worker.user_id);
                if (existingCliente != null)
                    throw new InvalidOperationException("The information is already registered.");

                // Registramos la información del trabajador y guardamos en la base de datos
                await _context.workers.AddAsync(worker);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task DeleteWorkerAsync(Guid id)
        {
            try
            {
                // Buscamos el usuario por su id
                var user = await _context.users.FindAsync(id);

                // Solo eliminamos si el usuario existe
                if (user != null)
                {
                    _context.users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al eliminar el trabajador en la base de datos
                throw new Exception("Error deleting the worker in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task UpdateWorkerAsync(UserUpdateDTO dto)
        {
            try
            {
                // Buscamos el usuario a actualizar
                var existing = await _context.users.FindAsync(dto.user_id);
                if (existing == null)
                    throw new InvalidOperationException("Worker not found");

                // Actualizamos solo los campos que llegan con valor (actualización parcial)
                if (!string.IsNullOrEmpty(dto.first_name))
                    existing.first_name = dto.first_name;

                if (!string.IsNullOrEmpty(dto.last_name))
                    existing.last_name = dto.last_name;

                if (!string.IsNullOrEmpty(dto.phone))
                    existing.phone = dto.phone;

                if (!string.IsNullOrEmpty(dto.gender))
                    existing.gender = dto.gender;

                if (dto.birth_date != null)
                    existing.birth_date = dto.birth_date;

                // Si cambió el nombre o apellido, regeneramos el username
                if (!string.IsNullOrEmpty(dto.first_name) || !string.IsNullOrEmpty(dto.last_name))
                {
                    string firstName = dto.first_name?.Split(' ')[0] ?? "";
                    string lastName = dto.last_name?.Split(' ')[0] ?? "";
                    existing.username = $"{firstName} {lastName}";
                }

                // Si llega una nueva imagen, reemplazamos la anterior
                if (dto.image != null && dto.image.Length > 0)
                {
                    // Borramos la imagen anterior si existe
                    if (!string.IsNullOrEmpty(existing.image_url))
                    {
                        _imageService.DeleteImageAsync(existing.image_url);
                    }
                    var img = await _imageService.SaveImageAsync(dto.image, "users");
                    existing.image_url = img;
                }

                // Guardamos los cambios en la base de datos
                if (existing != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task UpdateInfoWorkerAsync(WorkerDTO dto)
        {
            try
            {
                // Validamos que lleguen datos
                if (dto == null)
                    throw new ArgumentException("Invalid data");

                // Buscamos la información del trabajador a actualizar
                var existing = await _context.workers
                    .Where(p => p.user_id == dto.user_id)
                    .FirstOrDefaultAsync();

                if (existing == null)
                    throw new InvalidOperationException("Worker not found");

                // Actualizamos solo los campos que llegan con valor (actualización parcial)
                if (!string.IsNullOrWhiteSpace(dto.description))
                    existing.description = dto.description;

                if (dto.latitude != null)
                    existing.latitude = dto.latitude;

                if (dto.longitude != null)
                    existing.longitude = dto.longitude;

                if (dto.km_cost != null)
                    existing.km_cost = dto.km_cost;

                if (dto.range_km != null)
                    existing.range_km = dto.range_km;

                if (!string.IsNullOrWhiteSpace(dto.city))
                    existing.city = dto.city;

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Relanzamos el error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllWorkersAsync()
        {
            try
            {
                // Obtenemos todos los usuarios de la base de datos
                var users = await _context.users.ToListAsync();

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Devolvemos la lista de usuarios
                return users;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los trabajadores
                throw new Exception("Error retrieving the workers list", ex);
            }
        }

        public async Task<object?> GetWorkerByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el usuario trabajador activo con sus métricas
                var result = await (
                    from s in _context.users
                    join w in _context.workers on s.user_id equals w.user_id
                    where s.is_active == true && s.user_id == id
                    select new
                    {
                        s.user_id,
                        s.username,
                        s.first_name,
                        s.last_name,
                        // URL de la imagen del usuario
                        Imagen = string.IsNullOrEmpty(s.image_url)
                            ? null
                            : baseUrl + s.image_url,
                        // Total de trabajos realizados (jobs + express)
                        workers = (_context.jobs
                            .Where(t => t.worker_id == w.worker_id)
                            .Count()) + (_context.express
                            .Where(t => t.worker_id == w.worker_id)
                            .Count()),
                        // Total de servicios publicados por el trabajador
                        services = (_context.services)
                            .Where(s => s.worker_id == w.worker_id)
                            .Count(),
                        s.phone,
                        s.gender,
                        s.birth_date,
                        s.email,
                        w.is_working,
                        registered = FechaHelper.GetTiempoRelativo(s.created_at),
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el trabajador, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el trabajador por id
                throw new Exception("Error retrieving the worker by id", ex);
            }
        }

        public async Task<object?> GetInfoWorkerByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos la información del trabajador (ubicación, costos, etc.)
                var result = await (
                    from w in _context.workers
                    join s in _context.users on w.user_id equals s.user_id
                    where s.is_active == true && w.user_id == id
                    select new
                    {
                        w.user_id,
                        w.description,
                        w.latitude,
                        w.longitude,
                        w.km_cost,
                        w.city,
                        w.range_km,
                        w.is_working
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró la información, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar la información del trabajador
                throw new Exception("Error retrieving the worker info by id", ex);
            }
        }

        public async Task<object?> GetProfileWorker(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el perfil completo del trabajador con reseñas y evidencias
                var result = await (
                    from w in _context.workers
                    join u in _context.users on w.user_id equals u.user_id
                    where w.user_id == id
                    select new
                    {
                        w.user_id,
                        w.description,
                        w.rating,
                        w.city,
                        u.username,
                        u.first_name,
                        u.last_name,
                        u.gender,
                        w.is_working,
                        registered = FechaHelper.GetTiempoRelativo(u.created_at),
                        // URL de la imagen del trabajador
                        Image = string.IsNullOrEmpty(u.image_url)
                            ? null
                            : baseUrl + u.image_url,
                        // Total de trabajos activos realizados (jobs + express)
                        workers = (_context.jobs
                            .Where(t => t.worker_id == w.worker_id && t.is_active == true)
                            .Count()) + (_context.express
                            .Where(t => t.worker_id == w.worker_id && t.is_active == true)
                            .Count()),
                        // Total de servicios publicados por el trabajador
                        service = (_context.services)
                            .Where(s => s.worker_id == w.worker_id)
                            .Count(),
                        // Reseñas que ha recibido el trabajador
                        Review = (
                            from r in _context.reviews_workers
                            where r.worker_id == w.worker_id
                            select new
                            {
                                r.rating,
                                r.comment,
                                registered = FechaHelper.GetTiempoRelativo(r.created_at),
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
                            }).ToList(),
                        // Evidencias de los trabajos realizados por el trabajador
                        Evidence = (
                            from e in _context.evidence
                            join j in _context.jobs on e.job_id equals j.job_id
                            where j.worker_id == w.worker_id
                            select
                                string.IsNullOrEmpty(e.image_url)
                                    ? null
                                    : baseUrl + e.image_url
                        ).ToList()
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el perfil, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el perfil del trabajador
                throw new Exception("Error retrieving the worker profile", ex);
            }
        }

        public async Task UpdateActiveWorker(Guid id)
        {
            try
            {
                // Buscamos la información del trabajador
                var existing = await _context.workers
                    .Where(p => p.user_id == id)
                    .FirstOrDefaultAsync();

                // Alternamos su disponibilidad de trabajo (toggle)
                existing.is_working = !existing.is_working;

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