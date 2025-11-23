using System.Text;
using System.Text.Json.Serialization;
using DotNet.RateLimiter;
using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using DtuFoodAPI.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimitService(builder.Configuration);

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
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IGuidGenerator, GuidGenerator>();

// Validators
builder.Services.AddScoped<IValidator<UserRegistry>, UserRegistryValidator>();
builder.Services.AddScoped<IValidator<FoodTruckRegistry>, FoodTruckRegistryValidator>();
builder.Services.AddScoped<IValidator<ProductRegistry>, ProductRegistryValidator>();
builder.Services.AddScoped<IValidator<AvailabilityRegistry>, AvailabilityRegistryValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevents infinite cycles when serializing to json
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();

    app.UseSwaggerUi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<DtuFoodDbContext>();

    dbContext.Database.Migrate();
    //app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    //app.MapControllers().AllowAnonymous();
    app.MapControllers();
}
else
    app.MapControllers();
app.Run();