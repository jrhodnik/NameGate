using Microsoft.Extensions.DependencyInjection;
using NameGate.Database.Repositories;

namespace NameGate.Database
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddDbContext<MainContext>();
            services.AddHostedService<DatabaseBootstrapper>();

            services.AddScoped<IWhiteListRepository, EfcoreWhiteListRepository>();

            services.AddScoped<IIpStatisticsRepository, EfcoreIpStatisticsRepository>();
            services.AddScoped<IDomainStatisticsRepository, EfcoreDomainStatisticsRepository>();
            services.AddScoped<IBlockListBypassRepository, EfcoreBlockListBypassRepository>();

            return services;
        }
    }
}
