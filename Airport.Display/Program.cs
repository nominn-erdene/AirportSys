using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Airport.Core.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add SignalR client
builder.Services.AddSingleton<HubConnection>(sp =>
{
    var hubConnection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5268/flightHub")
        .WithAutomaticReconnect()
        .Build();

    return hubConnection;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Start SignalR connection
var hubConnection = app.Services.GetRequiredService<HubConnection>();
await hubConnection.StartAsync();

app.Run();
