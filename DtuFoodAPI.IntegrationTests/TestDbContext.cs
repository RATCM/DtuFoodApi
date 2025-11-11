using DtuFoodAPI.Database;
using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.IntegrationTests;

public class TestDbContext : DbContext, IDtuFoodDbContext
{
    public DbSet<FoodTruck> FoodTrucks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Availability> Availability { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
}