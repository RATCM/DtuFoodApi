using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Models;

public class Image
{
    public required Guid Id { get; init; }
    public required byte[] Blob { get; set; }
}