using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Database;

public class DataSeeder
{
    private readonly IDtuFoodDbContext _db;

    public DataSeeder(IDtuFoodDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Idempotent: if there are already trucks, assume seeded
        if (await _db.FoodTrucks.AnyAsync(cancellationToken))
            return;

        // Create a couple of FoodTrucks
        var tacoTruck = new FoodTruck
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Tasty Tacos",
            GpsLatitude = 55.785f,
            GpsLongitude = 12.523f,
            Products = new List<Product>(),
            Managers = new List<User>(),
            Availability = new List<Availability>()
        };

        var burgerTruck = new FoodTruck
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Big Burger Bus",
            GpsLatitude = 55.786f,
            GpsLongitude = 12.524f,
            Products = new List<Product>(),
            Managers = new List<User>(),
            Availability = new List<Availability>()
        };

        await _db.FoodTrucks.AddRangeAsync(new[] { tacoTruck, burgerTruck }, cancellationToken);

        // Products
        var products = new List<Product>
        {
            new()
            {
                FoodTruck = tacoTruck,
                Name = "Classic Taco",
                Description = "Beef taco with salsa and cheese",
                Price = 45.00m
            },
            new()
            {
                FoodTruck = tacoTruck,
                Name = "Veggie Taco",
                Description = "Grilled veggies, guacamole, pico de gallo",
                Price = 42.50m
            },
            new()
            {
                FoodTruck = burgerTruck,
                Name = "Cheeseburger",
                Description = "Beef patty, cheddar, pickles, house sauce",
                Price = 65.00m
            },
            new()
            {
                FoodTruck = burgerTruck,
                Name = "Chicken Burger",
                Description = "Crispy chicken, coleslaw, spicy mayo",
                Price = 62.00m
            }
        };
        await _db.Products.AddRangeAsync(products, cancellationToken);

        // Availability (Mon-Fri 11:00-14:00)
        var availabilities = new List<Availability>();
        foreach (var day in new[] { WeekDay.Monday, WeekDay.Tuesday, WeekDay.Wednesday, WeekDay.Thursday, WeekDay.Friday })
        {
            availabilities.Add(new Availability
            {
                Truck = tacoTruck,
                DayOfWeek = day,
                OpeningTime = new TimeOnly(11, 0),
                ClosingTime = new TimeOnly(14, 0)
            });
            availabilities.Add(new Availability
            {
                Truck = burgerTruck,
                DayOfWeek = day,
                OpeningTime = new TimeOnly(11, 0),
                ClosingTime = new TimeOnly(14, 0)
            });
        }

        await _db.Availability.AddRangeAsync(availabilities, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
