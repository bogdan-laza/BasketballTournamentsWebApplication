using BasketballTournamentsApp.Services;

namespace BasketballTournamentsApp.Extensions
{
    public static class CrudServiceExtension
    {
        public static IServiceCollection AddCrudServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSql(config);
            services.AddSingleton<TournamentService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<TeamService>();
            services.AddSingleton<TeamRegistrationService>();
            services.AddSingleton<TeamMemberRegistrationService>();
            services.AddSingleton<GameService>();

            return services;
        }
    }
}
