namespace BasketballTournamentsApp.Extensions
{
    public static class SqlExtensions
    {
        public static IServiceCollection AddSql(this IServiceCollection services, IConfiguration config)
        {
            var sqlConfig = new SqlConfig();
            config.GetSection("Sql").Bind(sqlConfig);
            services.AddSingleton(sqlConfig);
            return services;
        }
    }

    public class SqlConfig
    {
        public string ConnectionString { get; set; }
    }

}
