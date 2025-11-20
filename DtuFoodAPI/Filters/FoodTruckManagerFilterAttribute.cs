using System.Net;
using System.Security.Claims;
using DtuFoodAPI.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Filters;

public class FoodTruckManagerFilterAttribute : TypeFilterAttribute
{
    /// <summary>
    /// When applied to an endpoint or controller, it checks
    /// if the user is a manager of the food truck
    /// </summary>
    /// <param name="key">The name of the food truck id in the route template</param>
    /// <example>
    /// <code>
    /// [HttpPut("{id}")] // Route template
    /// [Authorize]
    /// [FoodTruckManagerFilter("id")] // Put the name of the id from the route template
    /// public async Task&lt;IActionResult&gt; UpdateFoodTruck(Guid id, [FromBody] FoodTruckRegistry foodTruck)
    /// {
    ///     ...
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// If the id is in the [Route] attribute instead (on the controller),
    /// then you should reference the id on the route attribute instead
    /// </remarks>
    public FoodTruckManagerFilterAttribute(string key) : base(typeof(FoodTruckManagerFilterService))
    {
        Arguments = [key];
        Order = 2;
    }
}

public class FoodTruckManagerFilterService : IAsyncResourceFilter
{
    private readonly string _key;
    private readonly ILogger<FoodTruckManagerFilterService> _logger;
    private readonly IDtuFoodDbContext _dbContext;
    
    public FoodTruckManagerFilterService(string key,
        ILogger<FoodTruckManagerFilterService> logger,
        IDtuFoodDbContext dbContext)
    {
        _key = key;
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var idClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        Guid userId = Guid.Parse(idClaim.Value);

        // This should generally never happen,
        // and it doesn't make sense to try and cover
        // this in tests
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

        var isManager = await _dbContext.FoodTrucks
            .Include(x => x.Managers)
            .Where(x => x.Id == truckIdGuid)
            .SelectMany(x => x.Managers)
            .AnyAsync(x => x.Id == userId);

        if (!isManager)
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.Forbidden,
                Message = "Only managers are allowed to access this resource"
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
            };

            return;
        }
        await next();
    }
}