using Microsoft.EntityFrameworkCore;
using DtuFoodAPI.Database;
using DtuFoodAPI.Models;

namespace DtuFoodAPI.Tests.Infra;

public class TestDtuFoodDbContext : DbContext, IDtuFoodDbContext
{
    public TestDtuFoodDbContext(DbContextOptions<TestDtuFoodDbContext> options) : base(options) { }

    public DbSet<FoodTruck> FoodTrucks { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Availability> Availabilities { get; set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);
}