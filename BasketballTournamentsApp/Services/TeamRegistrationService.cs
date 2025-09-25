using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using Microsoft.Data.SqlClient;

namespace BasketballTournamentsApp.Services
{
    public class TeamRegistrationService
    {
        private readonly SqlConfig _config;
        private readonly TournamentService _tournamentService;
        private readonly TeamService _teamService;

        public TeamRegistrationService(SqlConfig config, TournamentService tournamentService, TeamService teamService)
        {
            _config = config;
            _tournamentService = tournamentService;
            _teamService = teamService;
        }

        public async Task<List<TeamRegistration>> GetAllTeamRegistrationsAsync()
        {
            var teamRegistrations = new List<TeamRegistration>();

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_TEAM_REGISTRATIONS", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        teamRegistrations.Add(new TeamRegistration
                        {
                            TournamentId=reader.GetInt32(reader.GetOrdinal("TournamentId")),
                            TeamId = reader.GetInt32(reader.GetOrdinal("TeamId")),
                            RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
                        });
                    }
                }
            }
            return teamRegistrations;
        }

        public async Task AddTeamRegistrationAsync(TeamRegistration teamRegistration)
        {
            if (await _teamService.CheckIfTeamExistsByIdAsync(teamRegistration.TeamId) == false)
                throw new ArgumentException("The team does not exist!");

            if (await _tournamentService.CheckIfTournamentExistsByIdAsync(teamRegistration.TournamentId) == false)
                throw new ArgumentException("The tournament does not exist!");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_TEAM_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamRegistration.TeamId);
                cmd.Parameters.AddWithValue("@TournamentId", teamRegistration.TournamentId);
                cmd.Parameters.AddWithValue("@RegistrationDate", teamRegistration.RegistrationDate);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteTeamRegistrationAsync(int teamId, int tournamentId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("DELETE_TEAM_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.Parameters.AddWithValue("@TournamentId", tournamentId);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The TeamRegistration with TeamId {teamId} and TournamentId {tournamentId} does not exist!");
            }
        }

        public async Task UpdateTeamRegistrationAsync(TeamRegistration teamRegistration)
        {
            if (await _teamService.CheckIfTeamExistsByIdAsync(teamRegistration.TeamId) == false)
                throw new ArgumentException("The team does not exist!");

            if (await _tournamentService.CheckIfTournamentExistsByIdAsync(teamRegistration.TournamentId) == false)
                throw new ArgumentException("The tournament does not exist!");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_TEAM_REGISTRATION", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@TeamId", teamRegistration.TeamId);
                cmd.Parameters.AddWithValue("@TournamentId", teamRegistration.TournamentId);
                cmd.Parameters.AddWithValue("@RegistrationDate", teamRegistration.RegistrationDate);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"The teamRegistration with TeamId {teamRegistration.TeamId} and TournamentId {teamRegistration.TournamentId} does not exist!");
            }
        }

    }
}
