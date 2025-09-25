namespace BasketballTournamentsApp.Jwt
{

    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using BasketballTournamentsApp.Jwt;
    using Microsoft.IdentityModel.Tokens;

    public class TokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(JwtSettings settings) => _settings = settings;

        public string CreateAccessToken(int userId, string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username ?? string.Empty),
            new Claim(ClaimTypes.Role, role ?? "Player")
        };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public string HashRefreshToken(string refreshToken)
        {
            return BCrypt.Net.BCrypt.HashPassword(refreshToken);
        }

        public bool VerifyRefreshToken(string refreshToken, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(refreshToken, hashed);
        }
    }
}