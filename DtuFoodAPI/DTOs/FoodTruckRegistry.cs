using System.ComponentModel.DataAnnotations;

namespace DtuFoodAPI.DTOs;

public class FoodTruckRegistry
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    public required string Name { get; init; }
    
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public required float GpsLatitude { get; init; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degree")]
    public required float GpsLongitude { get; init; }
}