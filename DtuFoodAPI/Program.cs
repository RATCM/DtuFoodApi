using System.Text;
using System.Text.Json.Serialization;
using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// The program knows how to setup the authentication
// from the appsettings.json file
builder.Services.AddAuthentication(AuthSchemes.Access)
    .AddJwtBearer(AuthSchemes.Access)
    .AddJwtBearer(AuthSchemes.Refresh);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole("Admin"));
});

builder.Services.AddDbContext<IDtuFoodDbContext, DtuFoodDbContext>();

builder.Services.AddSingleton<IPasswordHasher<UserRegistry>, PasswordHasher<UserRegistry>>();
builder.Services.AddSingleton<IGuidGenerator, GuidGenerator>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFoodTruckService, FoodTruckService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IGuidGenerator, GuidGenerator>();

builder.Services.AddScoped<DataSeeder>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevents infinite cycles when serializing to json
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Apply migrations and seed in Development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<DtuFoodDbContext>();
        db.Database.Migrate();
        if (app.Environment.IsDevelopment())
        {
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration/seed error: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();