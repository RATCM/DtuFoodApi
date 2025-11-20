using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DtuFoodAPI.Models;

[Table("FoodTrucks")]
public class FoodTruck
{
    [Key]
    public required Guid Id { get; init; }
    
    [MaxLength(128)]
    public required string Name { get; set; }
    
    public required float GpsLatitude { get; set; }
    public required float GpsLongitude { get; set; }

    public Image? PageBanner { get; set; }
    public Image? HomeBanner { get; set; }
    public required ICollection<Product> Products { get; init; } = [];
    public required ICollection<User> Managers { get; init; } = [];
    public required ICollection<Availability> Availability { get; init; } = [];
}