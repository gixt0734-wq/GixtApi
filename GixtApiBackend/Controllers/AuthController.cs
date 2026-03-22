
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Infrastructure;
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

           

            // 🔥 GENERAR TOKEN CON VERSION
            var token = _tokenService.GenerateToken(
                user.user_id.ToString(),
                1
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

        

    }
}
