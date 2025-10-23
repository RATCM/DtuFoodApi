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

    public async Task<User> CreateUser(UserRegistry userRegistry, CancellationToken cancellationToken = default)
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

        return result.Entity;
    }

    public async Task<List<User>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetAllUsersWithRole(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.Where(x => x.Role == role).ToListAsync(cancellationToken);
    }

    public async Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email,
            cancellationToken: cancellationToken);
    }

    public async Task<User?> UpdateUser(Guid id, UserRegistry userRegistry, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id],  cancellationToken: cancellationToken);
        if (user is null) return null;
        
        user.Email = userRegistry.Email;
        user.PasswordHash = _userPasswordHasher.HashPassword(userRegistry, userRegistry.Password);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateUserRole(Guid id, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync([id],  cancellationToken: cancellationToken);
        if (user is null) return null;

        user.Role = newRole;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
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
    Task<User> CreateUser(UserRegistry userRegistry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All users</returns>
    Task<List<User>> GetAllUsers(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users with a specific role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Users with the role</returns>
    Task<List<User>> GetAllUsersWithRole(UserRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a user from a specific id
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user, or null if user doesn't exist</returns>
    Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a user from a specific email
    /// </summary>
    /// <param name="email">The user email</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user, or null if user doesn't exist</returns>
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user with a specific id with the data in the user registry
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="userRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated user, or null if the user doesn't exist</returns>
    /// <remarks>This will only update the email and password, to update the role use <see cref="UpdateUserRole"/> instead</remarks>
    Task<User?> UpdateUser(Guid id, UserRegistry userRegistry, CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Updates the user with a specific id with a new role
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="newRole">The new role</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated user, or null if the user doesn't exist</returns>
    Task<User?> UpdateUserRole(Guid id, UserRole newRole, CancellationToken cancellationToken = default);
    
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