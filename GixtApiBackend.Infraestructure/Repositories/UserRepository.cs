using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using GixtApiBackend.Application.DTos;


namespace GixtApi.Infraestructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailService _emailService;
        private readonly ImageService _imageService;
        public UserRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, EmailService emailService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _imageService = imageService;
        }

        public async Task<string>  VerficationEmailAsync(String Email )
        {
            var existingCliente = await _context.users.FirstOrDefaultAsync(c => c.email == Email);
            if (existingCliente != null)
                throw new InvalidOperationException("El correo ya está registrado.");

            if (Email == null)
            {
                throw new InvalidOperationException("Es necesario el correo.");
            }

            // 🔥 Generar código de 5 dígitos
            var rnd = new Random();
            var codigo = rnd.Next(10000, 99999).ToString();
            string body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Código de Verificación</title>
                </head>
                <body style="margin:0;padding:0;font-family:Arial,Helvetica,sans-serif;background-color:#f4f4f4;">
                    <table role="presentation" style="width:100%;border-collapse:collapse;">
                        <tr>
                            <td align="center" style="padding:40px 0;">
                                <table role="presentation" style="width:600px;max-width:100%;border-collapse:collapse;background-color:#ffffff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);">

                                    <tr>
                                        <td style="padding:40px 30px;text-align:center;border-bottom:1px solid #eeeeee; background-color: #1E6AE1;;">
                                            <h1 style="margin:0;color:#ffffff;font-size:27px;font-weight:bold; ">
                                                Código de Verificación
                                            </h1>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style="padding:40px 30px;">
                                            <p style="color:#666;font-size:16px;">
                                                Hola,
                                            </p>

                                            <p style="color:#666;font-size:16px;">
                                                Usa el siguiente código para completar tu verificación:
                                            </p>

                                            <div style="text-align:center;padding:20px;">
                                                <div style="background:#f8f9fa;border:2px dashed #dee2e6;border-radius:8px;padding:20px;display:inline-block;">
                                                    <span style="font-size:32px;font-weight:bold;letter-spacing:8px;color:#2c3e50;font-family:'Courier New',monospace;">
                                                        {codigo}
                                                    </span>
                                                </div>
                                            </div>

                                            <p style="color:#666;font-size:16px;">
                                                Este código expirará en <strong>3 minutos</strong>.
                                            </p>

                                            <p style="color:#666;font-size:14px;">
                                                Si no solicitaste este código, puedes ignorar este mensaje.
                                            </p>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style="padding:30px;background:#f8f9fa;border-top:1px solid #eee;text-align:center;">
                                            <p style="margin:0;color:#999;font-size:12px;">
                                                Este es un correo automático, por favor no respondas.
                                            </p>

                                            <p style="margin:10px 0 0 0;color:#999;font-size:12px;">
                                                © 2026 GIXT. Todos los derechos reservados.
                                            </p>
                                        </td>
                                    </tr>

                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;


            // ⚠️ Aquí deberías guardarlo en BD o cache (recomendado)
            // await _userRepository.SaveVerificationCode(dto.Email, codigo);

            // Enviar correo
            await _emailService.SendEmailAsync(Email, body);
            return codigo;
          
        }

        public async Task CreateUserAsync(UserDTO dto)
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

                user.rol_id = 1;
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
                                                            <li style="margin-bottom: 10px;">Solicitar servicios</li>
                                                            <li style="margin-bottom: 10px;">Explorar todas las funcionalidades disponibles</li>
                                                            <li style="margin-bottom: 10px;">Conectar con trabajadores</li>
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


        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _context.users.FindAsync(id);
            if (user != null)
            {
                _context.users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task UpdateUserAsync(UserUpdateDTO dto)
        {

            var existing = await _context.users.FindAsync(dto.user_id);
            if (existing == null)
                throw new Exception("Usuario no encontrado");

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

                var fullPath = Path.Combine(folder, nombreArchivo);

                await _imageService.SaveOptimizedImageAsync(dto.image, fullPath);

                // Guardar la ruta accesible desde la web
                existing.image_url = "/img/users/" + nombreArchivo;
            }

            if (existing != null)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _context.users.ToListAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";


            
            return users;
        }

        public async Task<Object?> GetUserByIdAsync(Guid id)
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
                }
            ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return result;
            
        }


    }
}
