using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;


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
            string? fullPath = null;

            try
            {
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

                user.rol_id = 3;
                user.is_active = true;
                user.password = BCrypt.Net.BCrypt.HashPassword(dto.password);

                var existingCliente = await _context.users.FirstOrDefaultAsync(c => c.email == dto.email);
                if (existingCliente != null)
                    throw new InvalidOperationException("El correo ya está registrado.");

                string firstName = dto.first_name?.Split(' ')[0] ?? "";
                string lastName = dto.last_name?.Split(' ')[0] ?? "";

                string username = $"{firstName} {lastName}";
                user.username = username;


                if (dto.imagen != null && dto.imagen.Length > 0)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/users");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var nombreArchivo = $"{Guid.NewGuid()}.webp";
                    fullPath = Path.Combine(folder, nombreArchivo);

                    await _imageService.SaveOptimizedImageAsync(dto.imagen, fullPath);

                    // Guardar la ruta accesible desde la web
                    user.image_url = "/img/users/" + nombreArchivo;
                }
                await _context.users.AddAsync(user);
                await _context.SaveChangesAsync();

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
                                                                <td style="padding: 0 10px;">
                                                                    <a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Facebook</a>
                                                                </td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;">
                                                                    <a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Twitter</a>
                                                                </td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;">
                                                                    <a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">Instagram</a>
                                                                </td>
                                                                <td style="padding: 0 10px; color: #cccccc;">|</td>
                                                                <td style="padding: 0 10px;">
                                                                    <a href="#" style="color: #667eea; text-decoration: none; font-size: 14px;">LinkedIn</a>
                                                                </td>
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

                // Enviar correo
                _ = Task.Run(async () => _emailService.SendEmailAsync(dto.email, body));
            }
            catch (Exception)
            {
                // 🔥 ROLLBACK DE IMAGEN SI FALLA BD
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw; // relanza error
            }
        }

        public async Task CreateInfoWorkerAsync(WorkerDTO dto)
        {

            try
            {
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

                worker.is_active = true;
                worker.rating = 0;

                var existingCliente = await _context.workers.FirstOrDefaultAsync(w => w.user_id == worker.user_id);
                if (existingCliente != null)
                    throw new InvalidOperationException("El la informacion ya está registrado.");

                await _context.workers.AddAsync(worker);
                await _context.SaveChangesAsync();


            }
            catch (Exception)
            {

                throw; // relanza error
            }
        }

        public async Task DeleteWorkerAsync(Guid id)
        {
            var user = await _context.users.FindAsync(id);
            if (user != null)
            {
                _context.users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateWorkerAsync(UserUpdateDTO dto)
        {

            var existing = await _context.users.FindAsync(dto.user_id);
            if (existing == null)
                throw new Exception("Worker no encontrado");

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

            if (!string.IsNullOrEmpty(dto.first_name) || !string.IsNullOrEmpty(dto.last_name))
            {
                string firstName = dto.first_name?.Split(' ')[0] ?? "";
                string lastName = dto.last_name?.Split(' ')[0] ?? "";

                existing.username = $"{firstName} {lastName}";
            }


            if (dto.image != null && dto.image.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/users");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                if (!string.IsNullOrEmpty(existing.image_url))
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existing.image_url.TrimStart('/')
                    );

                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
                var nombreArchivo = $"{Guid.NewGuid()}.webp";

                var imagePath = Path.Combine(folder, nombreArchivo);

                await _imageService.SaveOptimizedImageAsync(dto.image, imagePath);

                // Guardar la ruta accesible desde la web
                existing.image_url = "/img/users/" + nombreArchivo;
            }

            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateInfoWorkerAsync(WorkerDTO dto)
        {
            if (dto == null)
                throw new Exception("Datos inválidos");

            var existing = await _context.workers
                .Where(p => p.user_id == dto.user_id)
                .FirstOrDefaultAsync();

            if (existing == null)
                throw new Exception("Worker no encontrado");

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

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllWorkersAsync()
        {
            var users = await _context.users.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return users;
        }

        public async Task<object?> GetWorkerByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var result = await (
                from s in _context.users
                where s.is_active == true && s.user_id == id
                select new
                {
                    s.user_id,
                    s.username,
                    s.first_name,
                    s.last_name,
                    Imagen = string.IsNullOrEmpty(s.image_url)
                            ? null
                            : baseUrl + s.image_url,
                    s.phone,
                    s.gender,
                    s.birth_date,
                    s.email,
                    registered = FechaHelper.GetTiempoRelativo(s.created_at),
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;

        }

        public async Task<object?> GetInfoWorkerByIdAsync(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var result = await (
                from w in _context.workers
                where w.is_active == true && w.user_id == id
                select new
                {
                    w.user_id,
                    w.description,
                    w.latitude,
                    w.longitude,
                    w.km_cost,
                    w.city,
                    w.range_km

                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;

        }

        public async Task<object?> GetProfileWorker(Guid id)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
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
                   registered = FechaHelper.GetTiempoRelativo(u.created_at),
                   Image = string.IsNullOrEmpty(u.image_url)
                                     ? null
                                     : baseUrl + u.image_url,
                   Review =(
                   from r in _context.reviews_workers where r.worker_id == w.worker_id 
                   select new
                   {
                       r.rating,
                       r.comment,
                       registered = FechaHelper.GetTiempoRelativo(r.created_at),
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

            if (result == null)
                return null;

            return result;
        }
    }
}
