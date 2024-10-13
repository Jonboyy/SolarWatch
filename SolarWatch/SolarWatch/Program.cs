var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure named HttpClient for the Geocoding API
builder.Services.AddHttpClient("GeocodingClient", client =>
{
    client.BaseAddress = new Uri("http://api.openweathermap.org/geo/1.0/direct");
});

// Configure named HttpClient for the Sunrise-Sunset API
builder.Services.AddHttpClient("SunsetClient", client =>
{
    client.BaseAddress = new Uri("https://api.sunrise-sunset.org/json");
});

// Add Swagger for API documentation and testing
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
app.Run(); // Start the application
