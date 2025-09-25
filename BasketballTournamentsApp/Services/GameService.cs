using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BasketballTournamentsApp.Services
{
    public class GameService
    {
        private readonly SqlConfig _config;
        private readonly TournamentService _tournamentService;
        private readonly TeamService _teamService;

        public GameService(SqlConfig config, TournamentService tournamentService, TeamService teamService)
        {
            _config = config;
            _tournamentService = tournamentService;
            _teamService = teamService;
        }

        public async Task<PagedResult<Game>> GetPagedGamesAsync(
    int page, int pageSize, string sortBy, string sortOrder, string? gameStatus)
        {
            if (page < 1) page = 1;
            const int maxPageSize = 100;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var allowedSortBy = new[] { "GameTime", "GameId" };
            if (!allowedSortBy.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                sortBy = "GameTime";

            if (sortOrder != "asc" && sortOrder != "desc")
                sortOrder = "desc";

            var items = new List<Game>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_GAMES_PAGED_SORT_FILTER", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SortBy", sortBy);
                cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                cmd.Parameters.AddWithValue("@GameStatus", (object?)gameStatus ?? DBNull.Value);
                var outParam = new SqlParameter("@TotalCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new Game
                        {
                            GameId = reader.GetInt32(reader.GetOrdinal("GameId")),
                            TournamentId = reader.GetInt32(reader.GetOrdinal("TournamentId")),
                            RoundNumber = reader.GetString(reader.GetOrdinal("RoundNumber")),
                            GameTime = reader.GetDateTime(reader.GetOrdinal("GameTime")),
                            Team1Id = reader.GetInt32(reader.GetOrdinal("Team1Id")),
                            Team2Id = reader.GetInt32(reader.GetOrdinal("Team2Id")),
                            ScoreTeam1 = reader.IsDBNull(reader.GetOrdinal("ScoreTeam1")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ScoreTeam1")),
                            ScoreTeam2 = reader.IsDBNull(reader.GetOrdinal("ScoreTeam2")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ScoreTeam2")),
                            GameStatus = reader.GetString(reader.GetOrdinal("GameStatus"))
                        });
                    }
                }
                totalCount = (int)(outParam.Value ?? 0);
            }

           

            return new PagedResult<Game>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
        }


        public async Task AddGameAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.RoundNumber))
                throw new ArgumentException("The round number is invalid!");

            if (await _tournamentService.CheckIfTournamentExistsByIdAsync(game.TournamentId) == false)
                throw new ArgumentException("The tournament does not exist!");

            if (await _teamService.CheckIfTeamExistsByIdAsync(game.Team1Id) == false)
                throw new ArgumentException("The first game does not exist!");

            if (await _teamService.CheckIfTeamExistsByIdAsync(game.Team2Id) == false)
                throw new ArgumentException("The second game does not exist!");

            if (game.GameStatus != "Completed" && game.ScoreTeam1 != null && game.ScoreTeam2 != null)
                throw new ArgumentException("The scores can only be set once the game is completed!");

            if (game.GameTime < (await _tournamentService.GetTournamentByIdAsync(game.TournamentId)).TournamentDate)
                throw new ArgumentException("Game cannot be scheduled before tournament start");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_GAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TournamentId", game.TournamentId);
                cmd.Parameters.AddWithValue("@RoundNumber", game.RoundNumber);
                cmd.Parameters.AddWithValue("@GameTime", game.GameTime);
                cmd.Parameters.AddWithValue("@Team1Id", game.Team1Id);
                cmd.Parameters.AddWithValue("@Team2Id", game.Team2Id);
                cmd.Parameters.AddWithValue("@ScoreTeam1", (object?)game.ScoreTeam1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ScoreTeam2", (object?)game.ScoreTeam2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GameStatus", game.GameStatus);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteGameAsync(int gameId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("DELETE_GAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@GameId", gameId);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The game with GameId {gameId} does not exist!");
            }
        }

        public async Task UpdateGameAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.RoundNumber))
                throw new ArgumentException("The round number is invalid!");

            if (await _tournamentService.CheckIfTournamentExistsByIdAsync(game.TournamentId) == false)
                throw new ArgumentException("The tournament does not exist!");

            if (await _teamService.CheckIfTeamExistsByIdAsync(game.Team1Id) == false)
                throw new ArgumentException("The first game does not exist!");

            if (await _teamService.CheckIfTeamExistsByIdAsync(game.Team2Id) == false)
                throw new ArgumentException("The second game does not exist!");

            if (game.GameStatus != "Completed" && game.ScoreTeam1 != null && game.ScoreTeam2 != null)
                throw new ArgumentException("The scores can only be set once the game is completed!");

            if (game.ScoreTeam1 < 0 || game.ScoreTeam2 < 0)
                throw new ArgumentException("Scores must be non-negative.");


            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_GAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@GameId", game.GameId);
                cmd.Parameters.AddWithValue("@TournamentId", game.TournamentId);
                cmd.Parameters.AddWithValue("@RoundNumber", game.RoundNumber);
                cmd.Parameters.AddWithValue("@GameTime", game.GameTime);
                cmd.Parameters.AddWithValue("@Team1Id", game.Team1Id);
                cmd.Parameters.AddWithValue("@Team2Id", game.Team2Id);
                cmd.Parameters.AddWithValue("@ScoreTeam1", (object?)game.ScoreTeam1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ScoreTeam2", (object?)game.ScoreTeam2 ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@GameStatus", game.GameStatus);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The game with GameId {game.GameId} does not exist!");
            }
        }

        public async Task UpdateScoresOfAGameAsync(Game game)
        {
            if (game.ScoreTeam1 < 0 || game.ScoreTeam2 < 0)
                throw new ArgumentException("Scores must be non-negative.");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_SCORES_OF_GAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@GameId", game.GameId);
                cmd.Parameters.AddWithValue("@ScoreTeam1", (object?)game.ScoreTeam1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ScoreTeam2", (object?)game.ScoreTeam2 ?? DBNull.Value);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The game with GameId {game.GameId} does not exist!");
            }
        }

        public async Task UpdateStatusOfAGameAsync(Game game)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_STATUS_OF_GAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@GameId", game.GameId);
                cmd.Parameters.AddWithValue("@GameStatus", game.GameStatus);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The game with GameId {game.GameId} does not exist!");
            }
        }
    }
}
