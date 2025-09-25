using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class TeamController:ControllerBase
    {
        private readonly TeamService _teamService;

        public TeamController(TeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeamsAsync(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] string sortBy = "CreatedAt",
       [FromQuery] string sortOrder = "desc",
       [FromQuery] string? teamName = null,
       [FromQuery] int? createdByUserId=null)
        {
            var paged = await _teamService.GetPagedTeamsAsync(page, pageSize, sortBy, sortOrder, teamName, createdByUserId);
            return Ok(paged);
        }



        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTeamAsync([FromBody] Team team)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _teamService.AddTeamAsync(team);
            return Ok("Team created successfully!");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamAsync([FromRoute] int id)
        {
            await _teamService.DeleteTeamAsync(id);
            return Ok(new { message = $"Team with id {id} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeamAsync([FromRoute] int id, [FromBody] Team team)
        {
            if (id != team.TeamId)
                return BadRequest(new { message = "Team ID mismatvh!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            await _teamService.UpdateTeamAsync(team);
            return Ok(new { message = $"Team with id {id} updated successfully!" });
        }
    }
}
