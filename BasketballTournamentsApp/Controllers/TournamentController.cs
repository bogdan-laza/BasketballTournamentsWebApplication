using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class TournamentController:ControllerBase
    {
        private readonly TournamentService _tournamentService;

        public TournamentController(TournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTournamentsAsync(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] string sortBy = "TournamentDate",
         [FromQuery] string sortOrder = "desc",
         [FromQuery] string? tournamentName = null,
         [FromQuery] string? tournamentLocation=null,
         [FromQuery] string? tournamentFormat=null,
         [FromQuery] int? createdByUserId=null,
         [FromQuery] string? status=null)
        {
            var paged = await _tournamentService.GetPagedTournamentsAsync(page, pageSize, sortBy, sortOrder, 
                tournamentName, tournamentLocation, tournamentFormat, createdByUserId, status);
            return Ok(paged);
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTournamentAsync([FromBody] Tournament tournament)
        {
            if (!ModelState.IsValid) 
            {
                return ValidationProblem(ModelState);
            }
            await _tournamentService.AddTournamentAsync(tournament);
            return Ok("Tournament created successfully");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournamentAsync([FromRoute] int id)
        {
            await _tournamentService.DeleteTournamentAsync(id);
            return Ok(new { message = $"Tournament with id {id} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTournamentAsync([FromRoute] int id, [FromBody] Tournament tournament)
        {

            if (id != tournament.TournamentId)
                return BadRequest(new { message = "Tournament ID mismatch." });
            await _tournamentService.UpdateTournamentAsync(tournament);
            return Ok(new { message = $"Tournament with id {id} updated successfully!" });
        }
    }
}
