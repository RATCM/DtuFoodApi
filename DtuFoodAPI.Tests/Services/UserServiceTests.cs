using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;

namespace DtuFoodAPI.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private TestDbContext _dbContext = null!;
    private UserService _service = null!;
    private IGuidGenerator _guidGenerator = null!;
    private IPasswordHasher<UserRegistry> _passwordHasher = null!;

    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);

        _guidGenerator = Substitute.For<IGuidGenerator>();
        _guidGenerator.NewGuid().Returns(_userId);

        _passwordHasher = Substitute.For<IPasswordHasher<UserRegistry>>();
        _passwordHasher.HashPassword(Arg.Any<UserRegistry>(), Arg.Any<string>())
            .Returns("HASHED_PASSWORD");

        _service = new UserService(_dbContext, _passwordHasher, _guidGenerator);
    }

    // ------------------------
    // HELPERS
    // ------------------------

    private UserRegistry CreateRegistry(string email = "test@test.com", string password = "1234")
    {
        return new UserRegistry
        {
            Email = email,
            Password = password
        };
    }

    private User CreateValidUser(string email = "test@test.com")
    {
        return new User
        {
            Id = _userId,
            Email = email,
            PasswordHash = "HASHED_PASSWORD",
            Role = UserRole.Manager,
            FoodTrucks = new List<FoodTruck>()
        };
    }

    // =======================
    // CREATE
    // =======================

    [Test]
    public async Task CreateUser_ShouldAddUserToDatabase()
    {
        var registry = CreateRegistry();

        var result = await _service.CreateUser(registry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_userId));
        Assert.That(result.Email, Is.EqualTo("test@test.com"));

        var inDb = await _dbContext.Users.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Not.Null);
        Assert.That(inDb!.PasswordHash, Is.EqualTo("HASHED_PASSWORD"));
    }

    // =======================
    // GET ALL
    // =======================

    [Test]
    public async Task GetAllUsers_ShouldReturnAll()
    {
        _dbContext.Users.Add(CreateValidUser("user1@test.com"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllUsers();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Email, Is.EqualTo("user1@test.com"));
    }

    // =======================
    // GET BY ID
    // =======================

    [Test]
    public async Task GetUserById_WhenExists_ShouldReturnUser()
    {
        _dbContext.Users.Add(CreateValidUser("find@test.com"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetUserById(_userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("find@test.com"));
    }

    [Test]
    public async Task GetUserById_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetUserById(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // =======================
    // GET BY EMAIL
    // =======================

    [Test]
    public async Task GetUserByEmail_WhenExists_ShouldReturnUser()
    {
        _dbContext.Users.Add(CreateValidUser("mail@test.com"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetUserByEmail("mail@test.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("mail@test.com"));
    }

    [Test]
    public async Task GetUserByEmail_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetUserByEmail("none@test.com");

        Assert.That(result, Is.Null);
    }

    // =======================
    // UPDATE USER
    // =======================

    [Test]
    public async Task UpdateUser_WhenExists_ShouldUpdateFields()
    {
        _dbContext.Users.Add(CreateValidUser("old@test.com"));
        await _dbContext.SaveChangesAsync();

        var registry = CreateRegistry("new@test.com", "newpass");

        var result = await _service.UpdateUser(_userId, registry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("new@test.com"));

        var inDb = await _dbContext.Users.FirstAsync();
        Assert.That(inDb.Email, Is.EqualTo("new@test.com"));
    }

    [Test]
    public async Task UpdateUser_WhenNotExists_ShouldReturnNull()
    {
        var registry = CreateRegistry();

        var result = await _service.UpdateUser(Guid.NewGuid(), registry);

        Assert.That(result, Is.Null);
    }

    // =======================
    // UPDATE ROLE
    // =======================

    [Test]
    public async Task UpdateUserRole_WhenExists_ShouldUpdateRole()
    {
        _dbContext.Users.Add(CreateValidUser());
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateUserRole(_userId, UserRole.Admin);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Role, Is.EqualTo(UserRole.Admin.ToString()));
    }

    [Test]
    public async Task UpdateUserRole_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.UpdateUserRole(Guid.NewGuid(), UserRole.Admin);

        Assert.That(result, Is.Null);
    }

    // =======================
    // PASSWORD
    // =======================

    [Test]
    public async Task GetPasswordHash_WhenExists_ShouldReturnHash()
    {
        _dbContext.Users.Add(CreateValidUser());
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetPasswordHash(_userId);

        Assert.That(result, Is.EqualTo("HASHED_PASSWORD"));
    }

    [Test]
    public async Task GetPasswordHash_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetPasswordHash(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // =======================
    // DELETE
    // =======================

    [Test]
    public async Task DeleteUser_WhenExists_ShouldReturnTrue()
    {
        _dbContext.Users.Add(CreateValidUser());
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteUser(_userId);

        Assert.That(result, Is.True);

        var inDb = await _dbContext.Users.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Null);
    }

    [Test]
    public async Task DeleteUser_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.DeleteUser(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    // =======================
    // EXISTS
    // =======================

    [Test]
    public async Task UserIdExists_WhenExists_ShouldReturnTrue()
    {
        _dbContext.Users.Add(CreateValidUser());
        await _dbContext.SaveChangesAsync();

        var result = await _service.UserIdExists(_userId);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserIdExists_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.UserIdExists(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UserEmailExists_WhenExists_ShouldReturnTrue()
    {
        _dbContext.Users.Add(CreateValidUser("exists@test.com"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.UserEmailExists("exists@test.com");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserEmailExists_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.UserEmailExists("no@test.com");

        Assert.That(result, Is.False);
    }

    [TearDown]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }
}
