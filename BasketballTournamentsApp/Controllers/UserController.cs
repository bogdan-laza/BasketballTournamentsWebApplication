using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController:ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        //[Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? username = null,
        [FromQuery] string? email = null,
        [FromQuery] string? personRole = null)
        {
            var paged = await _userService.GetPagedUsersAsync(page, pageSize, sortBy, sortOrder,
                username, email, personRole);
            return Ok(paged);
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> AddUserAsync([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            await _userService.AddUserAsync(user);
            return Ok("User created successfully");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync([FromRoute] int id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = $"User with id {id} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync([FromRoute] int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest("User ID mismatch!");
            }

            await _userService.UpdateUserAsync(user);
            return Ok(new { message = $"User with id {id} updated successfully!" });
        }
    }
}
