using DtuFoodAPI.Database;
using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Tests; // or IntegrationTests if you prefer

public class TestDbContext : DbContext, IDtuFoodDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<FoodTruck> FoodTrucks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Availability> Availability { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
}