using System.Net;
using System.Security.Claims;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DtuFoodAPI.Filters;

public class UserFilterAttribute : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string _key;
    private readonly bool _allowAdmins;
    
    /// <summary>
    /// When applied to an endpoint or controller, it checks
    /// the logged-in user, if it has the same id as the one in the endpoint
    /// </summary>
    /// <param name="key">The name of the food truck id in the route template</param>
    /// <example>
    /// <code>
    /// [HttpPut("{id}")] // Route template
    /// [Authorize]
    /// [UserFilter("id")] // Put the name of the id from the route template
    /// public async Task&lt;IActionResult&gt; UpdateUser(Guid id, [FromBody] UserRegistry user)
    /// {
    ///     ...
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// If the id is in the [Route] attribute instead (on the controller),
    /// then you should reference the id on the route attribute instead
    /// </remarks>
    public UserFilterAttribute(string key, bool allowAdmins = false)
    {
        _key = key;
        _allowAdmins = allowAdmins;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        Console.WriteLine("UserFilter...");
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        Console.WriteLine($"Id claim: {userIdClaim?.Value}");
        if (userIdClaim is null)
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Message = "You need to be logged-in to access this resource"
            };
            
            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
            };
            return;
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var providedIdStr = context.HttpContext.GetRouteValue(_key) as string;
        Console.WriteLine($"Provided id str: {providedIdStr}");
        if (providedIdStr is null)
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.BadRequest,
            };
            
            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
            return;
        }
        var isAdmin = context.HttpContext.User.IsInRole(nameof(UserRole.Admin));
        var providedId = Guid.Parse(providedIdStr);
        Console.WriteLine(isAdmin);
        var canAccess = userId == providedId || (isAdmin && _allowAdmins);
        if ( !canAccess )
        {
            var errorResponse = new
            {
                Status = (int)HttpStatusCode.Forbidden,
                Message = "You are not allowed to update other users"
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
