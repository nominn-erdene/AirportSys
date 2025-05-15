using System.Threading.Tasks;
using System.Collections.Generic;
using Airport.Core.Models;

namespace Airport.Core.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<Flight>> GetAllFlightsAsync();
        Task<Flight> GetFlightByIdAsync(int id);
        Task<Flight> GetFlightByNumberAsync(string flightNumber);
        Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus);
        Task<bool> AssignSeatToPassengerAsync(int flightId, string seatNumber, string passportNumber);
        Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId);
        Task<bool> IsSeatAvailableAsync(int flightId, string seatNumber);
    }
} 