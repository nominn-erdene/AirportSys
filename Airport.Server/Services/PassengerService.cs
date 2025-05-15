using System;
using System.Threading.Tasks;
using System.Linq;
using Airport.Core.Models;
using Airport.Core.Interfaces;
using Airport.Data.Interfaces;

namespace Airport.Server.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly IRepository<Passenger> _passengerRepository;
        private readonly IRepository<Flight> _flightRepository;
        private readonly IRepository<BoardingPass> _boardingPassRepository;
        private readonly IFlightService _flightService;
        private readonly IFlightNotificationHub _notificationHub;

        public PassengerService(
            IRepository<Passenger> passengerRepository,
            IRepository<Flight> flightRepository,
            IRepository<BoardingPass> boardingPassRepository,
            IFlightService flightService,
            IFlightNotificationHub notificationHub)
        {
            _passengerRepository = passengerRepository;
            _flightRepository = flightRepository;
            _boardingPassRepository = boardingPassRepository;
            _flightService = flightService;
            _notificationHub = notificationHub;
        }

        public async Task<Passenger> GetPassengerByPassportAsync(string passportNumber)
        {
            var passengers = await _passengerRepository.FindAsync(p => p.PassportNumber == passportNumber);
            return passengers.FirstOrDefault();
        }

        public async Task<bool> CheckInPassengerAsync(string passportNumber, int flightId, string seatNumber)
        {
            var passenger = await GetPassengerByPassportAsync(passportNumber);
            if (passenger == null) return false;

            var success = await _flightService.AssignSeatToPassengerAsync(flightId, seatNumber, passportNumber);
            if (success)
            {
                await _notificationHub.NotifyCheckInCompleted(flightId, passportNumber);
            }

            return success;
        }

        public async Task<bool> GenerateBoardingPassAsync(string passportNumber, int flightId)
        {
            var passenger = await GetPassengerByPassportAsync(passportNumber);
            if (passenger == null) return false;

            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null) return false;

            var seat = await GetPassengerSeatAsync(passportNumber, flightId);
            if (seat == null) return false;

            var boardingPass = new BoardingPass
            {
                PassengerName = $"{passenger.FirstName} {passenger.LastName}",
                PassportNumber = passenger.PassportNumber,
                FlightNumber = flight.FlightNumber,
                Destination = flight.Destination,
                DepartureTime = flight.DepartureTime,
                Gate = flight.Gate,
                SeatNumber = seat.SeatNumber,
                CheckInTime = DateTime.UtcNow,
                BoardingGroup = DetermineBoardingGroup(seat.SeatNumber)
            };

            boardingPass.BarCode = boardingPass.GenerateBarCode();

            await _boardingPassRepository.AddAsync(boardingPass);
            return await _boardingPassRepository.SaveChangesAsync();
        }

        public async Task<Seat> GetPassengerSeatAsync(string passportNumber, int flightId)
        {
            var passenger = await GetPassengerByPassportAsync(passportNumber);
            if (passenger == null) return null;

            return passenger.AssignedSeat;
        }

        private string DetermineBoardingGroup(string seatNumber)
        {
            // Simple logic to determine boarding group based on seat number
            // You can implement more complex logic based on your requirements
            if (seatNumber.StartsWith("A") || seatNumber.StartsWith("B"))
                return "1";
            else if (seatNumber.StartsWith("C") || seatNumber.StartsWith("D"))
                return "2";
            else
                return "3";
        }
    }
} 