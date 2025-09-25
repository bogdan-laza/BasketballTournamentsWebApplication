using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BasketballTournamentsApp.Services
{
    public class TeamService
    {
        private readonly SqlConfig _config;
        private readonly UserService _userService;

        public TeamService(SqlConfig config, UserService userService)
        {
            _config = config;
            _userService = userService;
        }

        public async Task<PagedResult<Team>> GetPagedTeamsAsync(
   int page, int pageSize, string sortBy, string sortOrder, string? teamName, int? CreatedByUserId)
        {
            if (page < 1) page = 1;
            const int maxPageSize = 100;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var allowedSortBy = new[] { "CreatedAt", "TeamId" };
            if (!allowedSortBy.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                sortBy = "CreatedAt";

            if (sortOrder != "asc" && sortOrder != "desc")
                sortOrder = "desc";

            var items = new List<Team>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_TEAMS_PAGED_SORT_FILTER", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SortBy", sortBy);
                cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                cmd.Parameters.AddWithValue("@TeamName", (object?)teamName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedByUserId", (object?)CreatedByUserId ?? DBNull.Value);
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
                        items.Add(new Team
                        {
                            TeamId = reader.GetInt32(reader.GetOrdinal("TeamId")),
                            TeamName = reader.GetString(reader.GetOrdinal("TeamName")),
                            CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString(reader.GetOrdinal("LogoUrl"))
                        });
                    }
                }
                totalCount = (int)(outParam.Value ?? 0);
            }



            return new PagedResult<Team>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
        }

        public async Task AddTeamAsync(Team team)
        {
            if (string.IsNullOrWhiteSpace(team.TeamName))
                throw new ArgumentException("The team name is invalid!");

            if (await _userService.CheckIfUserExistsByIdAsync(team.CreatedByUserId) == false)
                throw new ArgumentException("The user does no exist!");

            using(var conn=new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_TEAM", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamName", team.TeamName);
                cmd.Parameters.AddWithValue("@CreatedByUserId", team.CreatedByUserId);
                cmd.Parameters.AddWithValue("@CreatedAt", team.CreatedAt);
                cmd.Parameters.AddWithValue("@LogoUrl", (object?)team.LogoUrl ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteTeamAsync(int teamId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using(var cmd=new SqlCommand("DELETE_TEAM", conn))
            {
                cmd.CommandType=System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                var rowsAffected= await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The team with TeamId {teamId} does not exist!");
            }
        }

        public async Task UpdateTeamAsync(Team team)
        {
            if (string.IsNullOrWhiteSpace(team.TeamName))
                throw new ArgumentException("The team name is invalid!");

            if (await _userService.CheckIfUserExistsByIdAsync(team.CreatedByUserId) == false)
                throw new ArgumentException("The user does no exist!");

            using(var conn=new SqlConnection(_config.ConnectionString))
            using(var cmd=new SqlCommand("UPDATE_TEAM", conn))
            {
                cmd.CommandType=System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", team.TeamId);
                cmd.Parameters.AddWithValue("@TeamName", team.TeamName);
                cmd.Parameters.AddWithValue("@CreatedByUserId", team.CreatedByUserId);
                cmd.Parameters.AddWithValue("@CreatedAt", team.CreatedAt);
                cmd.Parameters.AddWithValue("@LogoUrl", (object?)team.LogoUrl ?? DBNull.Value);

                var rowsAffected=await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The team with TeamId {team.TeamId} does not exist!");
            }
        }

        public async Task<bool> CheckIfTeamExistsByIdAsync(int id)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("CHECK_IF_TEAM_EXISTS_BY_ID", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", id);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
}
