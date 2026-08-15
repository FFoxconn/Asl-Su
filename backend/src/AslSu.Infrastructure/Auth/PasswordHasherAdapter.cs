using AslSu.Application.Abstractions;
using AslSu.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AslSu.Infrastructure.Auth;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(User user, string password) => _inner.HashPassword(user, password);

    public bool Verify(User user, string hashedPassword, string providedPassword) =>
        _inner.VerifyHashedPassword(user, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
}
