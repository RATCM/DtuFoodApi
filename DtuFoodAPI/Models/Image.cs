using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DtuFoodAPI.Models;

[Table("Images")]
public class Image
{
    [Key] 
    public required Guid Id { get; init; }
    
    public required byte[] Blob { get; init; }
}