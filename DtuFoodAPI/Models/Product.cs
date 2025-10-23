using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Models;

[Table("Products")]
[PrimaryKey("TruckId", nameof(Name))]
public record Product
{
    [ForeignKey("TruckId")]
    public required FoodTruck FoodTruck { get; init; }
    
    [MaxLength(128)]
    public required string Name { get; set; }
    
    [MaxLength(1024)]
    public required string Description { get; set; }
    
    public required decimal Price { get; set; } // We use decimal instead of float to prevent precision errors
}