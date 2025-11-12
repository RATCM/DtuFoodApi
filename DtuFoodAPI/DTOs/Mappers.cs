using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

public static class Mappers
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            FoodTrucks = user.FoodTrucks.Select(x => x.Id).ToList()
        };
    }

    public static FoodTruckDto ToDto(this FoodTruck truck)
    {
        return new FoodTruckDto
        {
            Id = truck.Id,
            Name = truck.Name,
            GpsLatitude = truck.GpsLatitude,
            GpsLongitude = truck.GpsLongitude,
            Products = truck.Products.Select(x => x.ToDto()).ToList(),
            Availability = truck.Availability.Select(x => x.ToDto()).ToList()
        };
    }

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto()
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
        };
    }

    public static AvailabilityDto ToDto(this Availability availability)
    {
        return new AvailabilityDto()
        {
            DayOfWeek = availability.DayOfWeek.ToString(),
            OpeningTime = availability.OpeningTime,
            ClosingTime = availability.ClosingTime
        };
    }
    
}