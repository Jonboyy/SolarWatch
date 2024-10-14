using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container, including controllers
builder.Services.AddControllers();

// Configure named HttpClient for the Geocoding API
builder.Services.AddHttpClient("GeocodingClient", client =>
{
    // Base URL for the Geocoding API from configuration
    client.BaseAddress = new Uri(builder.Configuration["GeocodingAPI:BaseUrl"]);
});

// Configure named HttpClient for the Sunrise-Sunset API
builder.Services.AddHttpClient("SunsetClient", client =>
{
    // Base URL for the Sunrise-Sunset API from configuration
    client.BaseAddress = new Uri(builder.Configuration["SunriseSunsetAPI:BaseUrl"]);
});

// Add Swagger for API documentation (optional, but useful for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline for development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

