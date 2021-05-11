using DinkumCoin.Mining.Core.Repositories;
using DinkumCoin.Mining.Data.Contexts;
using DinkumCoin.Mining.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DinkumCoin.Mining.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMiningRepository, MiningRepository>();

            var connectionString = configuration.GetConnectionString("mining");
            services.AddDbContext<MiningDbContext>(options => options.UseSqlite(connectionString));

            return services;
        }
    }
}
