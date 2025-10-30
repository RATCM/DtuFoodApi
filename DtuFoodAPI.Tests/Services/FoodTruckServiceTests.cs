using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Services;
using DtuFoodAPI.Tests.Infra;

namespace DtuFoodAPI.Tests.Services;

public class FoodTruckServiceTests
{
    [Fact]
    public async Task CreateFoodTruck_Persists_And_Returns_Entity()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TestDtuFoodDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new TestDtuFoodDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var guid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var guidGen = new FixedGuidGenerator(guid);
        var service = new FoodTruckService(db, guidGen);

        var dto = new FoodTruckRegistry
        {
            Name = "Burger Baron",
            GpsLatitude = 55.7851f,
            GpsLongitude = 12.5211f
        };

        var created = await service.CreateFoodTruck(dto);

        Assert.Equal(guid, created.Id);
        Assert.Equal("Burger Baron", created.Name);
        Assert.Empty(created.Products);

        var fromDb = await db.FoodTrucks.SingleAsync();
        Assert.Equal(created.Id, fromDb.Id);
    }
}