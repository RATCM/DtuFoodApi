using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace DtuFoodAPI.Tests.Services;

[TestFixture]
public class ProductServiceTests
{
    private TestDbContext _dbContext;
    private ProductService _service;
    private IGuidGenerator _guidGenerator;

    private Guid _truckId;
    private Guid _productId;

    [SetUp]
    public void Setup()
    {
        _truckId = Guid.NewGuid();
        _productId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);

        _guidGenerator = Substitute.For<IGuidGenerator>();
        _guidGenerator.NewGuid().Returns(_productId);

        _service = new ProductService(_dbContext, _guidGenerator);
    }

    // ------------------------
    // HELPERS
    // ------------------------

    private FoodTruck CreateTruck()
    {
        return new FoodTruck
        {
            Id = _truckId,
            Name = "Test Truck",
            GpsLatitude = 55.0f,
            GpsLongitude = 12.0f,

            Products = new List<Product>(),
            Managers = new List<User>(),
            Availability = new List<Availability>()
        };
    }


    private ProductRegistry CreateRegistry(string name = "Burger")
    {
        return new ProductRegistry
        {
            Name = name,
            Price = "45.5",
            Description = "Nice burger",
            Category = "Food"
        };
    }

    private Product CreateValidProduct(string name = "Burger")
    {
        return new Product
        {
            Name = name,
            Price = 50,
            Description = "Test burger",
            Category = "Fast food",
            FoodTruck = CreateTruck(),

            Image = null // required
        };
    }


    // =======================
    // CREATE
    // =======================

    [Test]
    public async Task CreateProduct_WhenTruckExists_ShouldCreateProduct()
    {
        _dbContext.FoodTrucks.Add(CreateTruck());
        await _dbContext.SaveChangesAsync();

        var result = await _service.CreateProduct(_truckId, CreateRegistry());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Burger"));

        var inDb = await _dbContext.Products.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Not.Null);
    }

    [Test]
    public async Task CreateProduct_WhenTruckNotExists_ShouldReturnNull()
    {
        var result = await _service.CreateProduct(Guid.NewGuid(), CreateRegistry());

        Assert.That(result, Is.Null);
    }

    // =======================
    // GET ALL
    // =======================

    [Test]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        _dbContext.Products.Add(CreateValidProduct());
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllProducts();

        Assert.That(result.Count, Is.EqualTo(1));
    }
    

    // =======================
    // GET BY ID + NAME
    // =======================

    [Test]
    public async Task GetProductByTruckIdAndName_WhenExists_ShouldReturnProduct()
    {
        _dbContext.Products.Add(CreateValidProduct());
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetProductByTruckIdAndProductName(_truckId, "Burger");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Burger"));
    }

    // =======================
    // EXISTS
    // =======================

    [Test]
    public async Task ProductExists_WhenExists_ShouldReturnTrue()
    {
        _dbContext.Products.Add(CreateValidProduct());
        await _dbContext.SaveChangesAsync();

        var result = await _service.ProductExists(_truckId, "Burger");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ProductExists_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.ProductExists(_truckId, "Burger");

        Assert.That(result, Is.False);
    }

    // =======================
    // UPDATE
    // =======================

    [Test]
    public async Task UpdateProduct_WhenExists_ShouldUpdateFields()
    {
        _dbContext.Products.Add(CreateValidProduct());
        await _dbContext.SaveChangesAsync();

        var updated = new ProductRegistry
        {
            Name = "Burger",
            Price = "99.5",
            Description = "Better burger",
            Category = "Food"
        };

        var result = await _service.UpdateProduct(_truckId, "Burger", updated);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task UpdateProduct_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.UpdateProduct(_truckId, "Burger", CreateRegistry());

        Assert.That(result, Is.Null);
    }

    // =======================
    // DELETE
    // =======================

    [Test]
    public async Task DeleteProduct_WhenExists_ShouldReturnTrue()
    {
        _dbContext.Products.Add(CreateValidProduct());
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteProduct(_truckId, "Burger");

        Assert.That(result, Is.True);

        var inDb = await _dbContext.Products.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Null);
    }

    [Test]
    public async Task DeleteProduct_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.DeleteProduct(_truckId, "Burger");

        Assert.That(result, Is.False);
    }

    // =======================
    // IMAGE
    // =======================

    [Test]
    public async Task UpdateProductImage_WhenExists_ShouldSetImage()
    {
        var product = CreateValidProduct();
        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync();

        var bytes = new byte[] { 1, 2, 3, 4, 5 };

        var image = await _service.UpdateProductImage(_truckId, "Burger", bytes);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Blob, Is.EqualTo(bytes));
    }

    [TearDown]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }
}
