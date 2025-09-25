using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using Microsoft.Data.SqlClient;

namespace BasketballTournamentsApp.Services
{
    public class TeamMemberRegistrationService
    {
        private readonly SqlConfig _config;
        private readonly UserService _userService;
        private readonly TeamService _teamService;

        public TeamMemberRegistrationService(SqlConfig config, UserService userService, TeamService teamService)
        {
            _config = config;
            _userService = userService;
            _teamService = teamService;
        }

        public async Task<List<TeamMemberRegistration>> GetAllTeamMemberRegistrationsAsync()
        {
            var teamMemberRegistrations = new List<TeamMemberRegistration>();

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_TEAM_MEMBER_REGISTRATIONS", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        teamMemberRegistrations.Add(new TeamMemberRegistration
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            TeamId = reader.GetInt32(reader.GetOrdinal("TeamId")),
                            JoinedAt = reader.GetDateTime(reader.GetOrdinal("JoinedAt"))
                        });
                    }
                }
            }
            return teamMemberRegistrations;
        }

        public async Task AddTeamMemberRegistrationAsync(TeamMemberRegistration teamMemberRegistration)
        {
            if (await _teamService.CheckIfTeamExistsByIdAsync(teamMemberRegistration.TeamId) == false)
                throw new ArgumentException("The team does not exist!");

            if (await _userService.CheckIfUserExistsByIdAsync(teamMemberRegistration.UserId) == false)
                throw new ArgumentException("The user does not exist!");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_TEAM_MEMBER_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamMemberRegistration.TeamId);
                cmd.Parameters.AddWithValue("@UserId", teamMemberRegistration.UserId);
                cmd.Parameters.AddWithValue("@JoinedAt", teamMemberRegistration.JoinedAt);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteTeamMemberRegistrationAsync(int teamId, int userId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("DELETE_TEAM_MEMBER_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The TeamMemberRegistration with TeamId {teamId} and UserId {userId} does not exist!");
            }
        }

        public async Task UpdateTeamMemberRegistrationAsync(TeamMemberRegistration teamMemberRegistration)
        {
            if (await _teamService.CheckIfTeamExistsByIdAsync(teamMemberRegistration.TeamId) == false)
                throw new ArgumentException("The team does not exist!");

            if (await _userService.CheckIfUserExistsByIdAsync(teamMemberRegistration.UserId) == false)
                throw new ArgumentException("The user does not exist!");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_TEAM_MEMBER_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamMemberRegistration.TeamId);
                cmd.Parameters.AddWithValue("@UserId", teamMemberRegistration.UserId);
                cmd.Parameters.AddWithValue("@JoinedAt", teamMemberRegistration.JoinedAt);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The teamMemberRegistration with TeamId {teamMemberRegistration.TeamId} and UserId {teamMemberRegistration.UserId} does not exist!");
            }
        }

    }
}

