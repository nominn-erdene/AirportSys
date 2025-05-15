using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Airport.Server.Services;
using Airport.Core.Interfaces;
using Airport.Server.Hubs;
using Airport.Data.Interfaces;
using Airport.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Register repositories
builder.Services.AddSingleton<IRepository<Flight>, InMemoryRepository<Flight>>();
builder.Services.AddSingleton<IRepository<Passenger>, InMemoryRepository<Passenger>>();
builder.Services.AddSingleton<IRepository<Seat>, InMemoryRepository<Seat>>();
builder.Services.AddSingleton<IRepository<BoardingPass>, InMemoryRepository<BoardingPass>>();

// Add services
builder.Services.AddSingleton<IFlightService, FlightService>();
builder.Services.AddSingleton<IPassengerService, PassengerService>();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// Commenting out HTTPS redirection for development
//app.UseHttpsRedirection();

app.UseAuthorization();

// Map SignalR hub
app.MapHub<FlightHub>("/flightHub");

app.MapControllers();

app.Run();
