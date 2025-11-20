namespace DtuFoodAPI.Auth;

/// <summary>
/// Policies for authorization
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// Only admins can access the endpoint
    /// </summary>
    public const string AdminOnly = "AdminOnly";
}