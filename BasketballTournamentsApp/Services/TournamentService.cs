using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using BasketballTournamentsApp.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BasketballTournamentsApp.Services
{
    public class TournamentService
    {
        private readonly SqlConfig _config;
        private readonly UserService _userService;

        public TournamentService(SqlConfig config, UserService userService)
        {
            _config = config;
            _userService = userService;
        }

        public async Task<PagedResult<Tournament>> GetPagedTournamentsAsync(
            int page, int pageSize, string sortBy, string sortOrder, string? tournamentName, string? tournamentLocation, 
            string? tournamentFormat, int? createdByUserId, string? status)
        {
            if (page < 1)
                page = 1;
            const int maxPageSize = 100;
            if (pageSize < 1)
                pageSize = 10;
            if(pageSize>maxPageSize)
                pageSize = maxPageSize;

            var allowedSortBy = new[] { "TournamentDate", "EntryFee", "Prize", "TournamentId" };
            if (!allowedSortBy.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                sortBy = "TournamentDate";

            if(sortOrder!="desc" && sortOrder!="asc")
                sortOrder= "desc";

            var items=new List<Tournament>();
            int totalCount = 0;

            using (var conn=new SqlConnection(_config.ConnectionString))
            using (var cmd=new SqlCommand("GET_TOURNAMENTS_PAGED_SORT_FILTER", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SortBy", sortBy);
                cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                cmd.Parameters.AddWithValue("@TournamentName", (object?)tournamentName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TournamentLocation", (object?)tournamentLocation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TournamentFormat", (object?)tournamentFormat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedByUserId", (object?)createdByUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);

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
                        items.Add(new Tournament
                        {
                            TournamentId = reader.GetInt32(reader.GetOrdinal("TournamentId")),
                            TournamentName = reader.GetString(reader.GetOrdinal("TournamentName")),
                            TournamentDate = reader.GetDateTime(reader.GetOrdinal("TournamentDate")),
                            TournamentLocation = reader.GetString(reader.GetOrdinal("TournamentLocation")),
                            EntryFee = reader.GetDecimal(reader.GetOrdinal("EntryFee")),
                            Prize = reader.GetDecimal(reader.GetOrdinal("Prize")),
                            Rules = reader.IsDBNull(reader.GetOrdinal("Rules")) ? null : reader.GetString(reader.GetOrdinal("Rules")),
                            TournamentFormat = reader.GetString(reader.GetOrdinal("TournamentFormat")),
                            CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                            MaximumNumberOfTeams = reader.GetInt32(reader.GetOrdinal("MaximumNumberOfTeams")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            RegistrationDeadline = reader.GetDateTime(reader.GetOrdinal("RegistrationDeadline")),
                            TournamentImageUrl = reader.IsDBNull(reader.GetOrdinal("TournamentImageUrl")) ? null : reader.GetString(reader.GetOrdinal("TournamentImageUrl"))
                        });
                    }
                }
                totalCount = (int)(outParam.Value ?? 0);
            }

            return new PagedResult<Tournament>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
        }

        public async Task AddTournamentAsync(Tournament tournament)
        {
            if (tournament.RegistrationDeadline >= tournament.TournamentDate)
                throw new ArgumentException("Registration deadline must be before tournament date!");

            if (tournament.TournamentDate < DateTime.UtcNow)
                throw new ArgumentException("Tournament date cannot must be in the future!");

            if (tournament.RegistrationDeadline < DateTime.UtcNow)
                throw new ArgumentException("Registration deadline must be in the future!");

            if (await _userService.CheckIfUserExistsByIdAsync(tournament.CreatedByUserId) == false)
                throw new ArgumentException("The user id does not exist!");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_TOURNAMENT", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TournamentName", tournament.TournamentName);
                cmd.Parameters.AddWithValue("@TournamentDate", tournament.TournamentDate);
                cmd.Parameters.AddWithValue("@TournamentLocation", tournament.TournamentLocation);
                cmd.Parameters.AddWithValue("@EntryFee", tournament.EntryFee);
                cmd.Parameters.AddWithValue("@Prize", tournament.Prize);
                cmd.Parameters.AddWithValue("@Rules", tournament.Rules);
                cmd.Parameters.AddWithValue("@TournamentFormat", tournament.TournamentFormat);
                cmd.Parameters.AddWithValue("@CreatedByUserId", tournament.CreatedByUserId);
                cmd.Parameters.AddWithValue("@MaximumNumberOfTeams", tournament.MaximumNumberOfTeams);
                cmd.Parameters.AddWithValue("@Status", tournament.Status);
                cmd.Parameters.AddWithValue("@RegistrationDeadline", tournament.RegistrationDeadline);
                cmd.Parameters.AddWithValue("@TournamentImageUrl", (object?)tournament.TournamentImageUrl ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteTournamentAsync(int tournamentId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString)) 
            using(var cmd=new SqlCommand("DELETE_TOURNAMENT", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TournamentId", tournamentId);
                var rowsAffected=await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new ArgumentException($"Tournament with ID {tournamentId} does not exist.");
                }
            }
        }

        public async Task UpdateTournamentAsync(Tournament tournament)
        {
            if (tournament.RegistrationDeadline >= tournament.TournamentDate)
                throw new ArgumentException("Registration deadline must be before tournament date!");

            if (tournament.TournamentDate < DateTime.UtcNow)
                throw new ArgumentException("Tournament date must be in the future!");

            if (tournament.RegistrationDeadline < DateTime.UtcNow)
                throw new ArgumentException("Registration deadline must be in the future!");

            using (var conn=new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_TOURNAMENT", conn))
            {
                cmd.CommandType=System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TournamentId", tournament.TournamentId);
                cmd.Parameters.AddWithValue("@TournamentName", tournament.TournamentName);
                cmd.Parameters.AddWithValue("@TournamentDate", tournament.TournamentDate);
                cmd.Parameters.AddWithValue("@TournamentLocation", tournament.TournamentLocation);
                cmd.Parameters.AddWithValue("@EntryFee", tournament.EntryFee);
                cmd.Parameters.AddWithValue("@Prize", tournament.Prize);
                cmd.Parameters.AddWithValue("@Rules", tournament.Rules);
                cmd.Parameters.AddWithValue("@TournamentFormat", tournament.TournamentFormat);
                cmd.Parameters.AddWithValue("@CreatedByUserId", tournament.CreatedByUserId);
                cmd.Parameters.AddWithValue("@MaximumNumberOfTeams", tournament.MaximumNumberOfTeams);
                cmd.Parameters.AddWithValue("@Status", tournament.Status);
                cmd.Parameters.AddWithValue("@RegistrationDeadline", tournament.RegistrationDeadline);
                cmd.Parameters.AddWithValue("@TournamentImageUrl", (object?)tournament.TournamentImageUrl ?? DBNull.Value);

                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if(rowsAffected==0)
                {
                    throw new ArgumentException($"Tournament with ID {tournament.TournamentId} does not exist.");
                }
            }

        }

        public async Task<bool> CheckIfTournamentExistsByIdAsync(int id)
        {
            using(var conn=new SqlConnection(_config.ConnectionString)) 
            using(var cmd=new SqlCommand("CHECK_IF_TOURNAMENT_EXISTS_BY_ID", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TournamentId", id);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public async Task<Tournament?> GetTournamentByIdAsync(int tournamentId)
        {
            using var conn = new SqlConnection(_config.ConnectionString);
            using var cmd = new SqlCommand("GET_TOURNAMENT_BY_ID", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", tournamentId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new Tournament
            {
                TournamentId = reader.GetInt32(reader.GetOrdinal("TournamentId")),
                TournamentName = reader.GetString(reader.GetOrdinal("TournamentName")),
                TournamentDate = reader.GetDateTime(reader.GetOrdinal("TournamentDate")),
                TournamentLocation = reader.GetString(reader.GetOrdinal("TournamentLocation")),
                EntryFee = reader.GetDecimal(reader.GetOrdinal("EntryFee")),
                Prize = reader.GetDecimal(reader.GetOrdinal("Prize")),
                Rules = reader.IsDBNull(reader.GetOrdinal("Rules")) ? null : reader.GetString(reader.GetOrdinal("Rules")),
                TournamentFormat = reader.GetString(reader.GetOrdinal("TournamentFormat")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                MaximumNumberOfTeams = reader.GetInt32(reader.GetOrdinal("MaximumNumberOfTeams")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                RegistrationDeadline = reader.GetDateTime(reader.GetOrdinal("RegistrationDeadline")),
                TournamentImageUrl = reader.IsDBNull(reader.GetOrdinal("TournamentImageUrl")) ? null : reader.GetString(reader.GetOrdinal("TournamentImageUrl"))
            };
        }
    }
}
