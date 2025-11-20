using System.Net;
using DtuFoodAPI.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Filters;

public class FoodTruckExistsFilterAttribute : TypeFilterAttribute
{
    public FoodTruckExistsFilterAttribute(string key) : base(typeof(FoodTruckExistsFilterService))
    {
        Arguments = [key];
        Order = 1;
    }
}

public class FoodTruckExistsFilterService : IAsyncResourceFilter
{
    private readonly string _key;
    private readonly IDtuFoodDbContext _dbContext;

    public FoodTruckExistsFilterService(string key,
        IDtuFoodDbContext dbContext)
    {
        _key = key;
        _dbContext = dbContext;
    }
    
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.HttpContext.GetRouteValue(_key) is not string truckIdStr)
            throw new Exception("Truck id not matching route parameter");
        
        var truckIdGuid = Guid.Parse(truckIdStr);
        
        if (!await _dbContext.FoodTrucks.AnyAsync(x => x.Id == truckIdGuid))
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.NotFound,
                Message = $"Food truck with id {truckIdGuid} not found"
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound,
            };

            return;
        }

        await next();
    }
}
