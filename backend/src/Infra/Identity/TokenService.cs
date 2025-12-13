using Fundo.Aplications.Aplication.Interfaces.Services;
using Fundo.Applications.Domain.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Fundo.Applications.Infra.Identity
{
    public sealed class TokenService : ITokenServices
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<string> CreateTokenAsync(AuthenticatedUser user)
        {
            _logger.LogInformation(
                "Token: generation started. UserId={UserId}",
                user.Id);

            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                _logger.LogError(
                    "Token: JWT configuration missing. HasKey={HasKey}, HasIssuer={HasIssuer}, HasAudience={HasAudience}",
                    !string.IsNullOrWhiteSpace(jwtKey),
                    !string.IsNullOrWhiteSpace(issuer),
                    !string.IsNullOrWhiteSpace(audience));

                throw new InvalidOperationException("JWT settings are missing (Jwt:Key/Issuer/Audience).");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation(
                "Token: generation finished. UserId={UserId}, ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                expires);

            return Task.FromResult(tokenString);
        }
    }
}
