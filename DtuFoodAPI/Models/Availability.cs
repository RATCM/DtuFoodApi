using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Models;

[Table("Availability")]
[PrimaryKey("TruckId", nameof(DayOfWeek))]
public class Availability
{
    [ForeignKey("TruckId")]
    public required FoodTruck Truck { get; init; }
    
    public required WeekDay DayOfWeek { get; init; }
    
    public required TimeOnly OpeningTime { get; set; }
    public required TimeOnly ClosingTime { get; set; }
}