using DataLayer.Context;
using DataLayer.Repo.Interfaces;
using DataLayer.Repo.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Extensions
{
    public static class SqlExtensions
    {
        public static IServiceCollection AddSqlDb(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ProductDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly("DataLayer")));

            services.AddScoped<IReadProductRepository, ReadProductRepository>();
            services.AddScoped<IWriteProductRepository, WriteProductRepository>();

            return services;
        }
    }
}