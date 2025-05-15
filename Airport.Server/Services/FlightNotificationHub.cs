using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Airport.Core.Models;
using Airport.Core.Interfaces;

namespace Airport.Server.Services
{
    public class FlightNotificationHub : Hub, IFlightNotificationHub
    {
        public async Task NotifyFlightStatusChanged(int flightId, FlightStatus newStatus)
        {
            await Clients.All.SendAsync("FlightStatusChanged", flightId, newStatus);
        }

        public async Task NotifySeatAssigned(int flightId, string seatNumber)
        {
            await Clients.All.SendAsync("SeatAssigned", flightId, seatNumber);
        }

        public async Task NotifyCheckInCompleted(int flightId, string passportNumber)
        {
            await Clients.All.SendAsync("CheckInCompleted", flightId, passportNumber);
        }

        public async Task SubscribeToFlightUpdates(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"flight_{flightNumber}");
        }

        public async Task UnsubscribeFromFlightUpdates(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"flight_{flightNumber}");
        }

        public async Task JoinFlightGroup(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, flightNumber);
        }

        public async Task LeaveFlightGroup(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, flightNumber);
        }
    }
} 