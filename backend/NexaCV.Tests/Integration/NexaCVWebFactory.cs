using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexaCV.Api.Data;

namespace NexaCV.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that spins up the full ASP.NET Core pipeline
/// against a unique in-memory database so each test class gets a clean slate.
/// </summary>
public class NexaCVWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Swap the shared in-memory DB for an isolated one (unique name per fixture)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));
        });
    }
}
