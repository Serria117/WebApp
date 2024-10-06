using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace WebApp.Services.CommonService;

public class JwtService(IConfiguration config)
{
    private readonly string _secretKey = config["JwtSettings:SecretKey"]!;
    private readonly string _issuer = config["JwtSettings:Issuer"]!;
    private readonly string _audience = config["JwtSettings:Audience"]!;
    private readonly int _expiryMinutes = int.Parse(config["JwtSettings:ExpiryMinutes"]!);
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateToken(User user, DateTime issuedAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenantId", "")
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: issuedAt.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );
        var jwt = _tokenHandler.WriteToken(token);
        return jwt;
    }

    public DateTime GetExpiration(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.ValidTo.ToLocalTime();
    }
}