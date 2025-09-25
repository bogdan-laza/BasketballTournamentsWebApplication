using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class TeamMemberRegistrationController:ControllerBase
    {
        private readonly TeamMemberRegistrationService _teamMemberRegistrationService;

        public TeamMemberRegistrationController(TeamMemberRegistrationService teamMemberRegistrationService)
        {
            _teamMemberRegistrationService = teamMemberRegistrationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeamMemberRegistrationAsync()
        {
            var teamMemberRegistrations = await _teamMemberRegistrationService.GetAllTeamMemberRegistrationsAsync();
            return Ok(teamMemberRegistrations);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTeamMemberRegistrationAsync([FromBody] TeamMemberRegistration teamMemberRegistration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _teamMemberRegistrationService.AddTeamMemberRegistrationAsync(teamMemberRegistration);
            return Ok("TeamMemberRegistration created successfully!");
        }

        [Authorize]
        [HttpDelete("{userId}/{teamId}")]
        public async Task<IActionResult> DeleteTeamMemberRegistrationAsync([FromRoute] int userId, [FromRoute] int teamId)
        {
            await _teamMemberRegistrationService.DeleteTeamMemberRegistrationAsync(userId, teamId);
            return Ok(new { message = $"TeamMemberRegistration with UserId {userId} and TeamId {teamId} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{userId}/{teamId}")]
        public async Task<IActionResult> UpdateTeamMemberRegistrationAsync([FromRoute] int userId, [FromRoute] int teamId, [FromBody] TeamMemberRegistration teamMemberRegistration)
        {
            if (userId != teamMemberRegistration.UserId)
                return BadRequest(new { message = "User ID mismatch!" });

            if (teamId != teamMemberRegistration.TeamId)
                return BadRequest(new { message = "Team ID mismatch!" });

            await _teamMemberRegistrationService.UpdateTeamMemberRegistrationAsync(teamMemberRegistration);
            return Ok(new { message = $"TeamMemberRegistration with UserId {userId} and TeamId {teamId} updated successfully!" });
        }
    }
}
