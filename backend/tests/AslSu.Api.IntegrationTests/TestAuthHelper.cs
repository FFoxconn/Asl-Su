using System.Net.Http.Headers;
using System.Net.Http.Json;
using AslSu.Application.Abstractions;
using AslSu.Application.Auth.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AslSu.Api.IntegrationTests;

public static class TestAuthHelper
{
    private const string Email = "test-user@aslsu.test";
    private const string Password = "Test-Password-123!";

    /// <summary>Seeds a user directly (if not already present) and returns an HttpClient
    /// carrying a valid Bearer token from a real login round trip.</summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(AslSuWebApplicationFactory factory)
    {
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            if (!dbContext.Users.Any(u => u.Email == Email))
            {
                var user = new User
                {
                    Email = Email,
                    DisplayName = "Test User",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                user.PasswordHash = passwordHasher.Hash(user, Password);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }
        }

        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(Email, Password));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }
}
