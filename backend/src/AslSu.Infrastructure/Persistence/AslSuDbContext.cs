using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.Persistence;

public class AslSuDbContext(DbContextOptions<AslSuDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StoreProductInventory> StoreProductInventories => Set<StoreProductInventory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<BatchRequestLog> BatchRequestLogs => Set<BatchRequestLog>();
    public DbSet<SyncLog> SyncLogs => Set<SyncLog>();
    public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AslSuDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
