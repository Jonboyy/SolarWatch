using Microsoft.Extensions.Configuration;
using SolarWatch.Services;

namespace SolarWatch;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IGeocodingService, OpenWeatherGeocodingService>();
        builder.Services.AddTransient<ISunriseSunsetService, SunriseSunsetApiService>();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}