using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Persistence;

public class AslSuDbContext(DbContextOptions<AslSuDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AslSuDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
