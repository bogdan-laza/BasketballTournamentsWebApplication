using BasketballTournamentsApp.Services;

namespace BasketballTournamentsApp.Jwt
{
    public interface IAuthService
    {
        Task<TokenResponse?> AuthenticateAsync(string username, string password);
        Task<TokenResponse?> RefreshAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(int userId); 
    }

    public class AuthService : IAuthService
    {
        private readonly TokenService _tokenService;
        private readonly UserService _userService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(TokenService tokenService, UserService userService, JwtSettings jwtSettings)
        {
            _tokenService = tokenService;
            _userService = userService;
            _jwtSettings = jwtSettings;
        }

        public async Task<TokenResponse?> AuthenticateAsync(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            var access = _tokenService.CreateAccessToken(user.UserId, user.Username, user.PersonRole);
            var refreshPlain = _tokenService.GenerateRefreshToken();
            var hashed = _tokenService.HashRefreshToken(refreshPlain);
            var expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresDays);

            await _userService.SetRefreshTokenAsync(user.UserId, hashed, expiry);

            return new TokenResponse { AccessToken = access, RefreshToken = refreshPlain };
        }

        public async Task<TokenResponse?> RefreshAsync(string refreshToken)
        {
            var user = await _userService.GetByRefreshTokenAsync(refreshToken, _tokenService);
            if (user == null) return null;

            if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return null;

            var access = _tokenService.CreateAccessToken(user.UserId, user.Username, user.PersonRole);
            var newRefreshPlain = _tokenService.GenerateRefreshToken();
            var hashed = _tokenService.HashRefreshToken(newRefreshPlain);
            var expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresDays);

            await _userService.SetRefreshTokenAsync(user.UserId, hashed, expiry);

            return new TokenResponse { AccessToken = access, RefreshToken = newRefreshPlain };
        }

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            await _userService.SetRefreshTokenAsync(userId, null, null); 
        }
    }

}
