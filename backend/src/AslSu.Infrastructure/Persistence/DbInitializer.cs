using AslSu.Application.Abstractions;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AslSu.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>
    /// Development-only convenience: applies pending migrations and, if Seed:AdminEmail /
    /// Seed:AdminPassword are configured (via user-secrets) and no users exist yet, creates
    /// one admin account. No-op in other environments and a no-op if seed config is absent.
    /// </summary>
    public static async Task MigrateAndSeedAsync(
        AslSuDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger logger)
    {
        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "No users exist and Seed:AdminEmail/Seed:AdminPassword are not configured — " +
                "set them via user-secrets to seed a first admin account.");
            return;
        }

        var admin = new User
        {
            Email = adminEmail,
            DisplayName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        admin.PasswordHash = passwordHasher.Hash(admin, adminPassword);

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded initial admin account {Email}", adminEmail);
    }
}
