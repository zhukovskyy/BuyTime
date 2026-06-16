using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuyTime_Application.Common.Interfaces.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BuyTime_Infrastructure.Services;

public class JwtProvider(IConfiguration config) : IJwtProvider
{
    public string GenerateToken(BuyTime_Domain.Entities.User user)
    {
        var secret = config["Jwt:Secret"] ?? throw new Exception("JWT Secret is missing in appsettings.json");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("TelegramId", user.TelegramChatId),
            new Claim("IsExpert", user.IsExpert.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}