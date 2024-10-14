using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient("GeocodingClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GeocodingAPI:BaseUrl"]);
});

builder.Services.AddHttpClient("SunsetClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SunriseSunsetAPI:BaseUrl"]);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

