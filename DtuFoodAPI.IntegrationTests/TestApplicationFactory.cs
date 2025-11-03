using System.Data.Common;
using DtuFoodAPI.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DtuFoodAPI.IntegrationTests;

internal class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(IDbContextOptionsConfiguration<DtuFoodDbContext>));
            
            services.Remove(dbContextDescriptor!);
           
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbConnection));

            services.Remove(dbConnectionDescriptor!);
            services.RemoveAll<IDtuFoodDbContext>();
            
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });
            
            services.AddDbContext<IDtuFoodDbContext, TestDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });

        builder.UseEnvironment("Development");
    }
}