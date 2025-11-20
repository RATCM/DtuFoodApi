using System.Globalization;
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
    
    public static async Task<UserDto> ToDto(this Task<User> user)
    {
        return (await user).ToDto();
    }
    
    public static async ValueTask<UserDto> ToDto(this ValueTask<User> user)
    {
        return (await user).ToDto();
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
    
    public static async Task<FoodTruckDto> ToDto(this Task<FoodTruck> truck)
    {
        return (await truck).ToDto();
    }
    
    public static async ValueTask<FoodTruckDto?> ToDto(this ValueTask<FoodTruck?> truck)
    {
        return (await truck)?.ToDto();
    }

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto()
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price.ToString(CultureInfo.InvariantCulture),
            Category = product.Category,
        };
    }
    
    public static async Task<ProductDto> ToDto(this Task<Product> product)
    {
        return (await product).ToDto();
    }
    
    public static async ValueTask<ProductDto?> ToDto(this ValueTask<Product?> product)
    {
        return (await product)?.ToDto();
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
    
    public static async Task<AvailabilityDto> ToDto(this Task<Availability> availability)
    {
        return (await availability).ToDto();
    }
    
    public static async ValueTask<AvailabilityDto?> ToDto(this ValueTask<Availability?> availability)
    {
        return (await availability)?.ToDto();
    }    
}