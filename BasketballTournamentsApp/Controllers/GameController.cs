using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketballTournamentsApp.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class GameController:ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllGamesAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "GameTime",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? gameStatus = null)
        {
            var paged = await _gameService.GetPagedGamesAsync(page, pageSize, sortBy, sortOrder, gameStatus);
            return Ok(paged);
        }


       // [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddGameAsync([FromBody] Game game)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _gameService.AddGameAsync(game);
            return Ok("Game created successfully!");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGameAsync([FromRoute] int id)
        {
            await _gameService.DeleteGameAsync(id);
            return Ok(new { message = $"Game with id {id} deleted successfully!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGameAsync([FromRoute] int id, [FromBody] Game game)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            if (id != game.GameId)
                return BadRequest(new { message = "Game ID mismatch!" });

            await _gameService.UpdateGameAsync(game);
            return Ok(new { message = $"Game with id {id} updated successfully!" });
        }

        [Authorize]
        [HttpPatch("{id}/scores")]
        public async Task<IActionResult> UpdateScoresOfAGameAsync([FromRoute] int id, [FromBody] Game game)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != game.GameId)
                return BadRequest(new { message = "Game ID mismatch!" });

            await _gameService.UpdateScoresOfAGameAsync(game);
            return Ok(new { message = $"Scores of the game with id {id} updated successfully!" });
        }

        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatusOfAGameAsync([FromRoute] int id, [FromBody] Game game)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != game.GameId)
                return BadRequest(new { message = "Game ID mismatch!" });

            await _gameService.UpdateStatusOfAGameAsync(game);
            return Ok(new { message = $"Status of the game with id {id} updated successfully!" });
        }
    }
}
