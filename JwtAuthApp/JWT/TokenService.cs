using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthApp.JWT;

public class TokenService
{
    private readonly JwtConfiguration _config;

    public TokenService(JwtConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string id, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Email, email)
            // Add more claims if needed
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_config.ExpireDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}