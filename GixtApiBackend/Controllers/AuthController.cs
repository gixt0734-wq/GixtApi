
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Infraestructure;
using AutoMapper;
using GixtApiBackend.Domain.Entities;
namespace GixtApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context; // Cambiar por tu DbContext real
        private readonly TokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController( AppDbContext context, TokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("user")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDTO UserLoginDTO)
        {
            if (UserLoginDTO == null ||
                string.IsNullOrWhiteSpace(UserLoginDTO.Email) ||
                string.IsNullOrWhiteSpace(UserLoginDTO.Password))
            {
                return BadRequest(new { Message = "Faltan datos de inicio de sesión." });
            }

            var user = _context.users
                //.Where(u => u.rol_id == 1 || u.rol_id==2)
                .FirstOrDefault(c => c.email == UserLoginDTO.Email);

            if (user == null)
                return Unauthorized(new { Message = "El correo no está registrado." });

            if (!BCrypt.Net.BCrypt.Verify(UserLoginDTO.Password, user.password))
                return Unauthorized(new { Message = "La contraseña es incorrecta." });

            if (user.is_active == false)
                return Unauthorized(new { Message = "El usuario no puede acceder." });

            if (user.rol_id != 1 && user.rol_id != 2)
                return Unauthorized(new { Message = "El usuario no puede acceder." });

            //  BUSCAR O CREAR SESIÓN
            var session = await _context.sessions
                .FirstOrDefaultAsync(s => s.user_id == user.user_id);

            if (session == null)
            {
                session = new Session
                {
                    user_id = user.user_id,
                    device_id = UserLoginDTO.DeviceId,
                    device_name = UserLoginDTO.DeviceName,
                    token_fcm = UserLoginDTO.TokenFcm,
                    token_version = 1,
                    is_active = true
                };

                _context.sessions.Add(session);
            }
            else
            {
                // 🔥 LOGIN EN OTRO TELÉFONO → INVALIDA EL ANTERIOR
                session.device_id = UserLoginDTO.DeviceId;
                session.device_name = UserLoginDTO.DeviceName;
                session.token_fcm = UserLoginDTO.TokenFcm;
                session.token_version += 1;   // IMPORTANTE
                session.is_active = true;
                session.updated_at = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // 🔥 GENERAR TOKEN CON VERSION
            var token = _tokenService.GenerateToken(
                user.user_id.ToString(),
                session.token_version
            );

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}"; 

            return Ok(new
            {
                Token = token,
                Id = user.user_id,
                username = user.username,
                img = string.IsNullOrEmpty(user.image_url)
                        ? null
                        : baseUrl + user.image_url,
            });
        }

        [HttpPost("worker")]
        public async Task<IActionResult> LoginWorker([FromBody] UserLoginDTO UserLoginDTO)
        {
            if (UserLoginDTO == null ||
                string.IsNullOrWhiteSpace(UserLoginDTO.Email) ||
                string.IsNullOrWhiteSpace(UserLoginDTO.Password))
            {
                return BadRequest(new { Message = "Faltan datos de inicio de sesión." });
            }

            var user = _context.users
                .FirstOrDefault(c => c.email == UserLoginDTO.Email);


            if (user == null)
                return Unauthorized(new { Message = "El correo no está registrado." });

            if (!BCrypt.Net.BCrypt.Verify(UserLoginDTO.Password, user.password))
                return Unauthorized(new { Message = "La contraseña es incorrecta." });

            if (user.is_active == false)
                return Unauthorized(new { Message = "El usuario no puede acceder." });

            if (user.rol_id != 1 && user.rol_id != 3)
                return Unauthorized(new { Message = "El usuario no puede acceder." });

            //  BUSCAR O CREAR SESIÓN
            var session = await _context.sessions
                .FirstOrDefaultAsync(s => s.user_id == user.user_id);


            var info = _context.workers
                .Any(w => w.user_id == user.user_id && user.is_active == true);

            if (session == null)
            {
                session = new Session
                {
                    user_id = user.user_id,
                    device_id = UserLoginDTO.DeviceId,
                    device_name = UserLoginDTO.DeviceName,
                    token_fcm = UserLoginDTO.TokenFcm,
                    token_version = 1,
                    is_active = true
                };

                _context.sessions.Add(session);
            }
            else
            {
                // 🔥 LOGIN EN OTRO TELÉFONO → INVALIDA EL ANTERIOR
                session.device_id = UserLoginDTO.DeviceId;
                session.device_name = UserLoginDTO.DeviceName;
                session.token_fcm = UserLoginDTO.TokenFcm;
                session.token_version += 1;   // IMPORTANTE
                session.is_active = true;
                session.updated_at = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // 🔥 GENERAR TOKEN CON VERSION
            var token = _tokenService.GenerateToken(
                user.user_id.ToString(),
                session.token_version
            );

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return Ok(new
            {
                Token = token,
                Id = user.user_id,
                username = user.username,
                documents = false,
                info = info,
                img = string.IsNullOrEmpty(user.image_url)
                        ? null
                        : baseUrl + user.image_url,
            });
        }

    }
}
