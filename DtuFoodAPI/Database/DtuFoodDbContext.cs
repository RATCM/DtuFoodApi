using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DtuFoodAPI.Database;

public class DtuFoodDbContext : DbContext, IDtuFoodDbContext
{
    public DbSet<FoodTruck> FoodTrucks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Availability> Availability { get; set; }

    private readonly string _connectionString;
    
    public DtuFoodDbContext(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:Database"]!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .UseNpgsql(_connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This ensures that the product name is case-insensitive
        // such that we don't get duplicate names with different capitalization
        // e.g. "name1" would be the same as "Name1"
        modelBuilder.HasCollation(name: "case_insensitive", 
            locale: "und-u-ks-level2",
            provider: "icu", // Provider might be OS dependant
            deterministic: false);
        modelBuilder.Entity<Product>().Property(p => p.Name)
            .UseCollation("case_insensitive");
    }
}

public interface IDtuFoodDbContext 
{
    DbSet<FoodTruck> FoodTrucks { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Availability> Availability { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}