using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DtuFoodAPI.Tests.Services;

[TestFixture]
public class AvailabilityServiceTests
{
    private TestDbContext _dbContext = null!;
    private AvailabilityService _service = null!;

    private Guid _truckId;

    // =======================
    // SETUP
    // =======================

    [SetUp]
    public void Setup()
    {
        _truckId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);
        _service = new AvailabilityService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    // =======================
    // HELPERS
    // =======================

    private FoodTruck CreateTruck()
    {
        return new FoodTruck
        {
            Id = _truckId,
            Name = "Test Truck",
            GpsLatitude = 55,
            GpsLongitude = 12,
            Products = new List<Product>(),
            Managers = new List<User>(),
            Availability = new List<Availability>()
        };
    }

    private AvailabilityRegistry CreateValidRegistry(string day = "Monday")
    {
        return new AvailabilityRegistry
        {
            DayOfWeek = day,
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(18, 0)
        };
    }

    private Availability CreateAvailability(FoodTruck truck, WeekDay day)
    {
        return new Availability
        {
            Truck = truck,
            DayOfWeek = day,
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(18, 0)
        };
    }

    // =======================
    // CREATE
    // =======================

    [Test]
    public async Task CreateAvailabilityForTruck_WhenValid_ShouldAddAvailability()
    {
        var truck = CreateTruck();
        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var registry = CreateValidRegistry("Monday");

        var result = await _service.CreateAvailabilityForTruck(_truckId, registry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DayOfWeek, Is.EqualTo("Monday"));


        var inDb = await _dbContext.Availability.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Not.Null);
        Assert.That(inDb!.DayOfWeek, Is.EqualTo(WeekDay.Monday));
    }

    [Test]
    public async Task CreateAvailabilityForTruck_WhenTruckNotExists_ShouldReturnNull()
    {
        var registry = CreateValidRegistry();

        var result = await _service.CreateAvailabilityForTruck(Guid.NewGuid(), registry);

        Assert.That(result, Is.Null);
    }

    // =======================
    // GET ALL
    // =======================

    [Test]
    public async Task GetAllAvailabilityForTruck_WhenExists_ShouldReturnList()
    {
        var truck = CreateTruck();
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Monday));
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Tuesday));

        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllAvailabilityForTruck(_truckId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAvailabilityForTruck_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetAllAvailabilityForTruck(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // =======================
    // GET ONE
    // =======================

    [Test]
    public async Task GetAvailabilityForTruck_WhenExists_ShouldReturnDay()
    {
        var truck = CreateTruck();
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Wednesday));

        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAvailabilityForTruck(_truckId, WeekDay.Wednesday);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DayOfWeek, Is.EqualTo("Wednesday"));
    }

    [Test]
    public async Task GetAvailabilityForTruck_WhenNotExists_ShouldReturnNull()
    {
        var truck = CreateTruck();
        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAvailabilityForTruck(_truckId, WeekDay.Friday);

        Assert.That(result, Is.Null);
    }

    // =======================
    // UPDATE
    // =======================

    [Test]
    public async Task UpdateAvailabilityForTruck_WhenExists_ShouldUpdate()
    {
        var truck = CreateTruck();
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Monday));

        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var updated = new AvailabilityRegistry
        {
            DayOfWeek = "Tuesday",
            OpeningTime = new TimeOnly(12, 0),
            ClosingTime = new TimeOnly(20, 0)
        };

        var result = await _service.UpdateAvailabilityForTruck(_truckId, WeekDay.Monday, updated);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DayOfWeek, Is.EqualTo("Tuesday"));
        Assert.That(result.OpeningTime, Is.EqualTo(updated.OpeningTime));
        Assert.That(result.ClosingTime, Is.EqualTo(updated.ClosingTime));
    }

    [Test]
    public async Task UpdateAvailabilityForTruck_WhenTruckNotExists_ShouldReturnNull()
    {
        var result = await _service.UpdateAvailabilityForTruck(Guid.NewGuid(), WeekDay.Monday, CreateValidRegistry());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAvailabilityForTruck_WhenAvailabilityNotExists_ShouldReturnNull()
    {
        var truck = CreateTruck();
        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateAvailabilityForTruck(_truckId, WeekDay.Sunday, CreateValidRegistry());

        Assert.That(result, Is.Null);
    }

    // =======================
    // DELETE
    // =======================

    [Test]
    public async Task DeleteAvailability_WhenExists_ShouldReturnTrue()
    {
        var truck = CreateTruck();
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Friday));

        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteAvailability(_truckId, WeekDay.Friday);

        Assert.That(result, Is.True);

        var inDb = await _dbContext.Availability.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Null);
    }

    [Test]
    public async Task DeleteAvailability_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.DeleteAvailability(Guid.NewGuid(), WeekDay.Monday);

        Assert.That(result, Is.False);
    }

    // =======================
    // EXISTS
    // =======================

    [Test]
    public async Task AvailabilityExists_WhenExists_ShouldReturnTrue()
    {
        var truck = CreateTruck();
        truck.Availability.Add(CreateAvailability(truck, WeekDay.Thursday));

        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.AvailabilityExists(_truckId, WeekDay.Thursday);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AvailabilityExists_WhenNotExists_ShouldReturnFalse()
    {
        var truck = CreateTruck();
        _dbContext.FoodTrucks.Add(truck);
        await _dbContext.SaveChangesAsync();

        var result = await _service.AvailabilityExists(_truckId, WeekDay.Saturday);

        Assert.That(result, Is.False);
    }
}
