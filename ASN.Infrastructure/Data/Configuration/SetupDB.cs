using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASN.Infrastructure.Data.Configuration
{
    public static class SetupDB
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AsnContext>(options =>
            {
                options.UseSqlite("Data Source=databse.dat");

                // options.UseSqlServer(connectionString);
            });

            services.AddRepositories();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<UnitOfWork>();

            return services;
        }

        public static async Task<IServiceScope> SetupDBAsync(this IHost app)
        {
            var scope = app.Services.CreateScope();
            {
                AsnContext dbContext = scope.ServiceProvider.GetService<AsnContext>();
                await dbContext.Database.EnsureCreatedAsync();
                try
                {
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    // Ignore for SQLite Delete DB and start over 
                }
            }

            return scope;
        }
    }
}
