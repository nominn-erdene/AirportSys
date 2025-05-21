using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Airport.Server.Services;
using Airport.Core.Interfaces;
using Airport.Server.Hubs;
using Airport.Data.Interfaces;
using Airport.Core.Models;
using Airport.Data;
using Airport.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseUrls("http://localhost:5268");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth if needed
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Add DbContext with SQLite
builder.Services.AddDbContext<AirportDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

// Register repositories with EF Core
builder.Services.AddScoped<IRepository<Flight>, EfRepository<Flight>>();
builder.Services.AddScoped<IRepository<Passenger>, EfRepository<Passenger>>();
builder.Services.AddScoped<IRepository<Seat>, EfRepository<Seat>>();
builder.Services.AddScoped<IRepository<BoardingPass>, EfRepository<BoardingPass>>();
builder.Services.AddScoped<IRepository<Baggage>, EfRepository<Baggage>>();

// Add services
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddSingleton<IFlightNotificationHub, FlightHub>();

// Add Socket Server as a hosted service
builder.Services.AddHostedService<SocketServer>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed((host) => true)
               .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AirportDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
// Commenting out HTTPS redirection for development
//app.UseHttpsRedirection();

app.UseAuthorization();

// Map SignalR hub
app.MapHub<FlightHub>("/flightHub");

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
    throw;
}
