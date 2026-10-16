using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        "RaceDayTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d =>
                    d.ServiceType ==
                    typeof(DbContextOptions<RaceDayDbContext>) ||
                    d.ServiceType == typeof(RaceDayDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<RaceDayDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}