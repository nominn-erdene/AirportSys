using System.Threading.Tasks;
using Airport.Core.Models;

namespace Airport.Core.Interfaces
{
    public interface IFlightNotificationHub
    {
        Task NotifyFlightStatusChanged(int flightId, FlightStatus newStatus);
        Task NotifySeatAssigned(int flightId, string seatNumber);
        Task NotifyCheckInCompleted(int flightId, string passportNumber);
        Task SubscribeToFlightUpdates(string flightNumber);
        Task UnsubscribeFromFlightUpdates(string flightNumber);
    }
} 