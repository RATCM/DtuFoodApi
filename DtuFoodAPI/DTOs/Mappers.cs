using System.Globalization;
using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

/// <summary>
/// Mappers between different versions of the same entity
/// </summary>
public static class Mappers
{
    /// <summary>
    /// Convert User entity to a UserDto
    /// </summary>
    /// <param name="user">The user entity</param>
    /// <returns>The user DTO</returns>
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
    
    /// <summary>
    /// Async convert User entity to a UserDto
    /// </summary>
    /// <param name="user">The user entity task</param>
    /// <returns>The user DTO Task</returns>
    public static async Task<UserDto> ToDto(this Task<User> user)
    {
        return (await user).ToDto();
    }
    
    /// <summary>
    /// Async convert User entity to a UserDto using ValueTask
    /// </summary>
    /// <param name="user">The user entity ValueTask</param>
    /// <returns>The user DTO ValueTask</returns>
    public static async ValueTask<UserDto> ToDto(this ValueTask<User> user)
    {
        return (await user).ToDto();
    }

    /// <summary>
    /// Convert food truck entity to a FoodTruckDto
    /// </summary>
    /// <param name="truck">The food truck entity</param>
    /// <returns>The food truck DTO</returns>
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
    
    /// <summary>
    /// Async convert food truck entity to a FoodTruckDto
    /// </summary>
    /// <param name="truck">The food truck entity task</param>
    /// <returns>The food truck DTO task</returns>
    public static async Task<FoodTruckDto> ToDto(this Task<FoodTruck> truck)
    {
        return (await truck).ToDto();
    }
    
    /// <summary>
    /// Async convert food truck entity to a FoodTruckDto using ValueTask
    /// </summary>
    /// <param name="truck">The food truck entity ValueTask</param>
    /// <returns>The food truck DTO ValueTask</returns>
    public static async ValueTask<FoodTruckDto?> ToDto(this ValueTask<FoodTruck?> truck)
    {
        return (await truck)?.ToDto();
    }

    /// <summary>
    /// Convert product entity to a ProductDto
    /// </summary>
    /// <param name="product">The product entity</param>
    /// <returns>The product DTO</returns>
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
    
    /// <summary>
    /// Async convert product entity to a ProductDto
    /// </summary>
    /// <param name="product">The product entity task</param>
    /// <returns>The product DTO task</returns>
    public static async Task<ProductDto> ToDto(this Task<Product> product)
    {
        return (await product).ToDto();
    }
    
    /// <summary>
    /// Async convert product entity to a ProductDto using ValueTask
    /// </summary>
    /// <param name="product">The product entity ValueTask</param>
    /// <returns>The product DTO ValueTask</returns>
    public static async ValueTask<ProductDto?> ToDto(this ValueTask<Product?> product)
    {
        return (await product)?.ToDto();
    }

    /// <summary>
    /// Convert availability entity to a AvailabilityDto
    /// </summary>
    /// <param name="availability">The availability entity</param>
    /// <returns>The availability DTO</returns>
    public static AvailabilityDto ToDto(this Availability availability)
    {
        return new AvailabilityDto()
        {
            DayOfWeek = availability.DayOfWeek.ToString(),
            OpeningTime = availability.OpeningTime,
            ClosingTime = availability.ClosingTime
        };
    }
    
    /// <summary>
    /// Async convert availability entity to a AvailabilityDto
    /// </summary>
    /// <param name="availability">The availability entity task</param>
    /// <returns>The availability DTO task</returns>
    public static async Task<AvailabilityDto> ToDto(this Task<Availability> availability)
    {
        return (await availability).ToDto();
    }
    
    /// <summary>
    /// Async convert availability entity to a AvailabilityDto using ValueTask
    /// </summary>
    /// <param name="availability">The availability entity ValueTask</param>
    /// <returns>The availability DTO ValueTask</returns>
    public static async ValueTask<AvailabilityDto?> ToDto(this ValueTask<Availability?> availability)
    {
        return (await availability)?.ToDto();
    }    
}