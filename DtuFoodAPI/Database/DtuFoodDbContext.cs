using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace DtuFoodAPI.Database;

/// <summary>
/// Postgres database context
/// </summary>
public class DtuFoodDbContext : DbContext, IDtuFoodDbContext
{ 
    /// <inheritdoc />
    public DbSet<FoodTruck> FoodTrucks { get; set; }
    
    /// <inheritdoc />
    public DbSet<User> Users { get; set; }
    
    /// <inheritdoc />
    public DbSet<Product> Products { get; set; }
    
    /// <inheritdoc />
    public DbSet<Image> Images { get; set; }
    
    private readonly string _connectionString;
    
    /// <summary>
    /// Db context constructor
    /// </summary>
    /// <param name="configuration">The configuration</param>
    public DtuFoodDbContext(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:Database"]!;
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(_connectionString);

    /// <inheritdoc />
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

/// <summary>
/// Db context interface
/// </summary>
public interface IDtuFoodDbContext 
{
    /// <summary>
    /// The food trucks
    /// </summary>
    DbSet<FoodTruck> FoodTrucks { get; set; }
    
    /// <summary>
    /// The users
    /// </summary>
    DbSet<User> Users { get; set; }
    
    /// <summary>
    /// The food truck products
    /// </summary>
    DbSet<Product> Products { get; set; }
    
    /// <summary>
    /// The images
    /// </summary>
    DbSet<Image> Images { get; set; }
    
    /// <inheritdoc cref="DbContext.SaveChangesAsync(CancellationToken)"/>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}