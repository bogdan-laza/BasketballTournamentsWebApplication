using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class TeamRegistrationController:ControllerBase
    {
        private readonly TeamRegistrationService _teamRegistrationService;

        public TeamRegistrationController(TeamRegistrationService teamRegistrationService)
        {
            _teamRegistrationService = teamRegistrationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeamRegistrationAsync()
        {
            var teamRegistrations = await _teamRegistrationService.GetAllTeamRegistrationsAsync();
            return Ok(teamRegistrations);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTeamRegistrationAsync([FromBody] TeamRegistration teamRegistration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _teamRegistrationService.AddTeamRegistrationAsync(teamRegistration);
            return Ok("TeamRegistration created successfully!");
        }

        [Authorize]
        [HttpDelete("{tournamentId}/{teamId}")]
        public async Task<IActionResult> DeleteTeamRegistrationAsync([FromRoute] int tournamentId, [FromRoute] int teamId)
        {
            await _teamRegistrationService.DeleteTeamRegistrationAsync(tournamentId, teamId);
            return Ok(new { message = $"TeamRegistration with TournamentId {tournamentId} and TeamId {teamId} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{tournamentId}/{teamId}")]
        public async Task<IActionResult> UpdateTeamRegistrationAsync([FromRoute] int tournamentId, [FromRoute] int teamId, [FromBody] TeamRegistration teamRegistration)
        {
            if (tournamentId != teamRegistration.TournamentId)
                return BadRequest(new { message = "Tournament ID mismatch!" });

            if (teamId != teamRegistration.TeamId)
                return BadRequest(new { message = "Team ID mismatch!" });

            await _teamRegistrationService.UpdateTeamRegistrationAsync(teamRegistration);
            return Ok(new { message = $"TeamRegistration with TournamentId {tournamentId} and TeamId {teamId} updated successfully!" });
        }
    }
}

