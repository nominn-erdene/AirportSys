using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Airport.Core.Models;
using Airport.Core.Interfaces;

namespace Airport.Server.Hubs
{
    public class FlightHub : Hub, IFlightNotificationHub
    {
        private readonly IHubContext<FlightHub> _hubContext;

        public FlightHub(IHubContext<FlightHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyFlightStatusChanged(int flightId, FlightStatus newStatus)
        {
            await _hubContext.Clients.All.SendAsync("FlightStatusChanged", flightId, newStatus);
        }

        public async Task NotifySeatAssigned(int flightId, string seatNumber)
        {
            await _hubContext.Clients.All.SendAsync("SeatAssigned", flightId, seatNumber);
        }

        public async Task NotifyCheckInCompleted(int flightId, string passportNumber)
        {
            await _hubContext.Clients.All.SendAsync("CheckInCompleted", flightId, passportNumber);
        }

        public async Task SubscribeToFlightUpdates(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, flightNumber);
        }

        public async Task UnsubscribeFromFlightUpdates(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, flightNumber);
        }

        // Нислэгийн мэдээлэл өөрчлөгдөх үед дуудагдана
        public async Task UpdateFlightStatus(int flightId, FlightStatus newStatus)
        {
            await Clients.All.SendAsync("FlightStatusChanged", flightId, newStatus);
        }

        // Суудал захиалагдах үед дуудагдана
        public async Task UpdateSeatStatus(string flightNumber, string seatNumber, bool isOccupied)
        {
            await Clients.All.SendAsync("ReceiveSeatUpdate", flightNumber, seatNumber, isOccupied);
        }
    }
} 