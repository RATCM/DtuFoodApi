using System.Net;
using System.Security.Claims;
using DtuFoodAPI.Database;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Filters;

public class FoodTruckMangerOrAdminFilterAttribute : TypeFilterAttribute
{
    /// <summary>
    /// When applied to an endpoint or controller, it checks
    /// if the user is either a manager of the food truck, or an admin
    /// </summary>
    /// <param name="key">The name of the food truck id in the route template</param>
    /// <example>
    /// <code>
    /// [HttpPut("{id}")] // Route template
    /// [Authorize]
    /// [FoodTruckManagerOrAdminFilter("id")] // Put the name of the id from the route template
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
    public FoodTruckMangerOrAdminFilterAttribute(string key) : base(typeof(FoodTruckManagerFilterService))
    {
        Arguments = [key];
    }
}

public class FoodTruckManagerOrAdminFilterService : IAsyncAuthorizationFilter, IAsyncResourceFilter
{
    private readonly string _key;
    private readonly ILogger<FoodTruckManagerFilterService> _logger;
    private readonly IDtuFoodDbContext _dbContext;

    private bool _authResult = false;

    public FoodTruckManagerOrAdminFilterService(string key,
        ILogger<FoodTruckManagerFilterService> logger,
        IDtuFoodDbContext dbContext)
    {
        _key = key;
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        // Admins can just access the resource no matter what
        if (_authResult)
        {
            await next();
            return;
        }
        var idClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        
        switch (idClaim)
        {
            case null when !_authResult:
                return;
            case null:
                await next();
                return;
        }

        Guid userId = Guid.Parse(idClaim.Value);

        var truckId = context.HttpContext.GetRouteValue(_key) as Guid?;

        if (truckId == null) return;

        var isManager = await _dbContext.FoodTrucks
            .Include(x => x.Managers)
            .Where(x => x.Id == truckId)
            .SelectMany(x => x.Managers)
            .AnyAsync(x => x.Id == userId);

        if (!isManager)
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.Forbidden,
                Message = "Only managers and admins are allowed to access this resource"
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
            };

            return;
        }
        await next();
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var roleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role);

        _authResult = roleClaim?.Value == nameof(UserRole.Admin);
        return Task.CompletedTask;
    }
}