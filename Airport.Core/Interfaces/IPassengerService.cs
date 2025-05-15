using System.Threading.Tasks;
using Airport.Core.Models;

namespace Airport.Core.Interfaces
{
    public interface IPassengerService
    {
        Task<Passenger> GetPassengerByPassportNumber(string passportNumber);
        Task<BoardingPass> CheckInPassenger(int flightId, string passportNumber, string seatNumber);
        Task<bool> GenerateBoardingPassAsync(string passportNumber, int flightId);
        Task<Seat> GetPassengerSeatAsync(string passportNumber, int flightId);
    }
} 