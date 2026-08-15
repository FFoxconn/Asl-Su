using AslSu.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AslSu.Api.IntegrationTests;

/// <summary>
/// Swaps the SQL Server-backed AslSuDbContext for an in-memory SQLite database so the
/// full request pipeline (auth, EF Core, controllers) can be exercised without a real
/// SQL Server instance. Production code always targets SQL Server — this is test-only.
/// </summary>
public class AslSuWebApplicationFactory : WebApplicationFactory<AslSu.Api.Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "test-signing-key-for-integration-tests-only-not-a-real-secret-0123456789",
                ["Jwt:Issuer"] = "AslSu.Api.Tests",
                ["Jwt:Audience"] = "AslSu.Clients.Tests",
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Jwt:RefreshTokenDays"] = "14",
                ["ConnectionStrings:Default"] = "DataSource=:memory:",
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AslSuDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AslSuDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
