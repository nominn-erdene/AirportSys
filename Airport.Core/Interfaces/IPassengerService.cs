using System.Threading.Tasks;
using Airport.Core.Models;

namespace Airport.Core.Interfaces
{
    public interface IPassengerService
    {
        Task<Passenger> GetPassengerByPassportAsync(string passportNumber);
        Task<bool> CheckInPassengerAsync(string passportNumber, int flightId, string seatNumber);
        Task<bool> GenerateBoardingPassAsync(string passportNumber, int flightId);
        Task<Seat> GetPassengerSeatAsync(string passportNumber, int flightId);
    }
} 