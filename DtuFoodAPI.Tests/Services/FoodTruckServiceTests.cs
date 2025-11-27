using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace DtuFoodAPI.Tests.Services;

[TestFixture]
public class FoodTruckServiceTests
{
    private IDtuFoodDbContext _dbContext;
    private FoodTruckService _service;
    private IGuidGenerator _guidGenerator;

    private Guid _truckId;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _truckId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);


        _dbContext = new TestDbContext(options);

        _guidGenerator = Substitute.For<IGuidGenerator>();
        _guidGenerator.NewGuid().Returns(_truckId);

        _service = new FoodTruckService(_dbContext, _guidGenerator);
    }


    // ------------------------
    // HELPERS
    // ------------------------

    private FoodTruckRegistry CreateRegistry(string name = "TestTruck")
    {
        return new FoodTruckRegistry
        {
            Name = name,
            GpsLatitude = 55.0f,
            GpsLongitude = 12.0f
        };
    }

    private FoodTruck CreateValidTruck(string name = "Truck")
    {
        return new FoodTruck
        {
            Id = _truckId,
            Name = name,
            GpsLatitude = 55.0f,
            GpsLongitude = 12.0f,
            Products = new List<Product>(),
            Managers = new List<User>(),
            Availability = new List<Availability>()
        };
    }

    private User CreateValidUser()
    {
        return new User
        {
            Id = _userId,
            Email = "test@test.com",
            PasswordHash = "password",
            Role = UserRole.Admin,
            FoodTrucks = new List<FoodTruck>()
        };
    }

    // =======================
    // CREATE
    // =======================

    [Test]
    public async Task CreateFoodTruck_ShouldAddTruckToDatabase()
    {
        var registry = CreateRegistry();

        var result = await _service.CreateFoodTruck(registry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_truckId));
        Assert.That(result.Name, Is.EqualTo("TestTruck"));

        var inDb = await _dbContext.FoodTrucks
            .FirstOrDefaultAsync(t => t.Id == _truckId);

        Assert.That(inDb, Is.Not.Null);
    }

    // =======================
    // GET ALL
    // =======================

    [Test]
    public async Task GetAllFoodTrucks_ShouldReturnAll()
    {
        _dbContext.FoodTrucks.Add(CreateValidTruck("Truck 1"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllFoodTrucks();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Truck 1"));
    }

    // =======================
    // GET BY ID
    // =======================

    [Test]
    public async Task GetFoodTruckById_WhenExists_ShouldReturnTruck()
    {
        _dbContext.FoodTrucks.Add(CreateValidTruck("TruckX"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetFoodTruckById(_truckId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("TruckX"));
    }

    [Test]
    public async Task GetFoodTruckById_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetFoodTruckById(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    // =======================
    // UPDATE
    // =======================

    [Test]
    public async Task UpdateFoodTruck_WhenExists_ShouldUpdateFields()
    {
        _dbContext.FoodTrucks.Add(CreateValidTruck("Old Name"));
        await _dbContext.SaveChangesAsync();

        var newData = CreateRegistry("Updated Name");

        var result = await _service.UpdateFoodTruck(_truckId, newData);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateFoodTruck_WhenNotExists_ShouldReturnNull()
    {
        var registry = CreateRegistry();

        var result = await _service.UpdateFoodTruck(Guid.NewGuid(), registry);

        Assert.That(result, Is.Null);
    }

    // =======================
    // DELETE
    // =======================

    [Test]
    public async Task DeleteFoodTruck_WhenExists_ShouldReturnTrue()
    {
        _dbContext.FoodTrucks.Add(CreateValidTruck("DeleteMe"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteFoodTruck(_truckId);

        Assert.That(result, Is.True);

        var inDb = await _dbContext.FoodTrucks
            .FirstOrDefaultAsync(t => t.Id == _truckId);

        Assert.That(inDb, Is.Null);
    }

    [Test]
    public async Task DeleteFoodTruck_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.DeleteFoodTruck(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    // =======================
    // EXISTS
    // =======================

    [Test]
    public async Task FoodTruckExists_WhenExists_ShouldReturnTrue()
    {
        _dbContext.FoodTrucks.Add(CreateValidTruck("Exists"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.FoodTruckExists(_truckId);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task FoodTruckExists_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.FoodTruckExists(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    // =======================
    // ADD MANAGER
    // =======================

    [Test]
    public async Task AddFoodTruckManager_WhenValid_ShouldAddUser()
    {
        var truck = CreateValidTruck();
        var user = CreateValidUser();

        _dbContext.FoodTrucks.Add(truck);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _service.AddFoodTruckManager(_truckId, _userId);

        Assert.That(result, Is.Not.Null);

        var truckFromDb = await _dbContext.FoodTrucks
            .Include(t => t.Managers)
            .FirstAsync(t => t.Id == _truckId);

        Assert.That(truckFromDb.Managers.Count, Is.EqualTo(1));
    }
    
    [TearDown]
    public void TearDown()
    {
        if (_dbContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

}
