using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration configuration)
    {
        // Validar que las configuraciones necesarias existan
        _secretKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "El secreto del token JWT no está configurado.");
        _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer", "El emisor del token JWT no está configurado.");
        _audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience", "La audiencia del token JWT no está configurada.");
    }

    public string GenerateToken(string userId, int tokenVersion)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("El userId no puede ser nulo o vacío.", nameof(userId));
        }

        // Crear los claims del token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId), // Identificador único del usuario
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
            new Claim("user_id", userId),
            new Claim("token_version", tokenVersion.ToString())
        };

        // Generar la clave de seguridad a partir del secreto configurado
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Crear el token con sus propiedades
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(90), // Ajustar según tus necesidades
            signingCredentials: creds);

        // Retornar el token como un string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
