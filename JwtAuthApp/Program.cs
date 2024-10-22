using JwtAuthApp.JWT;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtConfiguration>();
builder.Services.AddTransient<TokenService>();
builder.Services.AddTransient<AppUser>();

// Add JWT Authentication configuration
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerGen(SwaggerConfiguration.Configure);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "NY", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Lviv"
};


app.MapPost("/login", (LoginRequest request, TokenService tokenService) =>
{
    // In a real app, you would validate the user's credentials against a database.
    // Authenticate user and generate token 
    // For demo purposes, we are using hardcoded values
    var userIsAuthenticated = request.Username == "admin" && request.Password == "admin";

    if (!userIsAuthenticated)
    {
        return Results.Unauthorized();
    }
    var userId = "9999"; // Get user id from database
    var email = "valentin.osidach@gmail.com"; // Get email from database
    var token = tokenService.GenerateToken(userId, email);

    return Results.Ok(token);
}).AllowAnonymous();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast")
.WithOpenApi();

//get user email from token 
app.MapGet("/user", [Authorize] (AppUser user) =>
{
    return Results.Ok(user.Email);
})
.RequireAuthorization()
.WithName("GetUserEmail")
.WithOpenApi();

app.Run();

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class AppUser : ClaimsPrincipal
{
    public AppUser(IHttpContextAccessor contextAccessor) : base(contextAccessor.HttpContext.User) { }

    public string Id => FindFirst(ClaimTypes.NameIdentifier).Value;
    public string Email => FindFirst(ClaimTypes.Email).Value;
}