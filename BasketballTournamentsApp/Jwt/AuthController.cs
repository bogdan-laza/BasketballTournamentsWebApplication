using BasketballTournamentsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BasketballTournamentsApp.Jwt
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var tokens = await _authService.AuthenticateAsync(req.Username, req.Password);
            if (tokens == null) return Unauthorized(new { message = "Invalid credentials" });
            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            var tokens = await _authService.RefreshAsync(req.RefreshToken);
            if (tokens == null) return Unauthorized(new { message = "Invalid or expired refresh token" });
            return Ok(tokens);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Revoke()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await (_authService).RevokeRefreshTokenAsync(userId);
            return NoContent();
        }
    }

}
