using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Services;

public class UserService : IUserService
{
    private readonly IDtuFoodDbContext _dbContext;
    private readonly IPasswordHasher<UserRegistry> _userPasswordHasher;
    private readonly IGuidGenerator _guidGenerator;
    
    public  UserService(IDtuFoodDbContext dbContext,
        IPasswordHasher<UserRegistry> userPasswordHasher,
        IGuidGenerator guidGenerator)
    {
        _dbContext = dbContext;
        _userPasswordHasher = userPasswordHasher;
        _guidGenerator = guidGenerator;
    }

    public async Task<UserDto> CreateUser(UserRegistry userRegistry, CancellationToken cancellationToken = default)
    {
        User user = new User()
        {
            Id = _guidGenerator.NewGuid(),
            Email = userRegistry.Email,
            PasswordHash = _userPasswordHasher.HashPassword(userRegistry, userRegistry.Password),
            Role = UserRole.Manager,
            FoodTrucks = new List<FoodTruck>()
        };

        var result = await _dbContext.Users.AddAsync(user, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return result.Entity.ToDto();
    }

    public async Task<List<UserDto>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.Include(x => x.FoodTrucks)
            .Select(x => x.ToDto()).ToListAsync(cancellationToken);
    }

    public async Task<List<UserDto>> GetAllUsersWithRole(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.Where(x => x.Role == role)
            .Include(x => x.FoodTrucks)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Users.Include(x => x.FoodTrucks).FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        return result?.ToDto();
    }

    public async Task<UserDto?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Users.Include(x => x.FoodTrucks).FirstOrDefaultAsync(x => x.Email == email,
            cancellationToken: cancellationToken);
        return result?.ToDto();
    }

    public async Task<UserDto?> UpdateUser(Guid id, UserRegistry userRegistry, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id],  cancellationToken: cancellationToken);
        if (user is null) return null;
        
        user.Email = userRegistry.Email;
        user.PasswordHash = _userPasswordHasher.HashPassword(userRegistry, userRegistry.Password);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto?> UpdateUserRole(Guid id, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.Include(x => x.FoodTrucks)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        if (user is null) return null;

        user.Role = newRole;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<string?> GetPasswordHash(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Users.FindAsync([id], cancellationToken: cancellationToken);
        return result?.PasswordHash;
    }

    public async Task<bool> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id], cancellationToken: cancellationToken);
        if (user is null) return false;
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<bool> UserIdExists(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync([id], cancellationToken: cancellationToken) is not null;
    }

    public async Task<bool> UserEmailExists(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }
}

public interface IUserService
{
    /// <summary>
    /// Creates a new user in the database from the user registry parameter
    /// </summary>
    /// <param name="userRegistry">The user registry</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The created user entity</returns>
    Task<UserDto> CreateUser(UserRegistry userRegistry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All users</returns>
    Task<List<UserDto>> GetAllUsers(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users with a specific role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Users with the role</returns>
    Task<List<UserDto>> GetAllUsersWithRole(UserRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a user from a specific id
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user, or null if user doesn't exist</returns>
    Task<UserDto?> GetUserById(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a user from a specific email
    /// </summary>
    /// <param name="email">The user email</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user, or null if user doesn't exist</returns>
    Task<UserDto?> GetUserByEmail(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user with a specific id with the data in the user registry
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="userRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated user, or null if the user doesn't exist</returns>
    /// <remarks>This will only update the email and password, to update the role use <see cref="UpdateUserRole"/> instead</remarks>
    Task<UserDto?> UpdateUser(Guid id, UserRegistry userRegistry, CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Updates the user with a specific id with a new role
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="newRole">The new role</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated user, or null if the user doesn't exist</returns>
    Task<UserDto?> UpdateUserRole(Guid id, UserRole newRole, CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Gets the password hash from a user with a specific id
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The password hash, or null if user is not found</returns>
    Task<string?> GetPasswordHash(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a user with a specific id
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user was deleted, false if the user was not found</returns>
    Task<bool> DeleteUser(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with a specific id exists
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user exists, otherwise false</returns>
    Task<bool> UserIdExists(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a user with a specific email exists
    /// </summary>
    /// <param name="email">The user email</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user exists, otherwise false</returns>
    Task<bool> UserEmailExists(string email, CancellationToken cancellationToken = default);
}