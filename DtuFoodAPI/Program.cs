using System.Text;
using System.Text.Json.Serialization;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// The program knows how to setup the authentication
// from the appsettings.json file
builder.Services.AddAuthentication("Access")
    .AddJwtBearer("Access")
    .AddJwtBearer("Refresh");

builder.Services.AddDbContext<IDtuFoodDbContext, DtuFoodDbContext>();

builder.Services.AddSingleton<IPasswordHasher<UserRegistry>, PasswordHasher<UserRegistry>>();
builder.Services.AddSingleton<IGuidGenerator, GuidGenerator>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFoodTruckService, FoodTruckService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IGuidGenerator, GuidGenerator>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevents infinite cycles when serializing to json
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();