using System;
using System.Threading.Tasks;
using System.Linq;
using Airport.Core.Models;
using Airport.Core.Interfaces;
using Airport.Data.Interfaces;
using Airport.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Airport.Server.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly IRepository<Passenger> _passengerRepository;
        private readonly IRepository<Flight> _flightRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<BoardingPass> _boardingPassRepository;
        private readonly IFlightService _flightService;
        private readonly IFlightNotificationHub _notificationHub;
        private readonly ILogger<PassengerService> _logger;

        public PassengerService(
            IRepository<Passenger> passengerRepository,
            IRepository<Flight> flightRepository,
            IRepository<Seat> seatRepository,
            IRepository<BoardingPass> boardingPassRepository,
            IFlightService flightService,
            IFlightNotificationHub notificationHub,
            ILogger<PassengerService> logger)
        {
            _passengerRepository = passengerRepository;
            _flightRepository = flightRepository;
            _seatRepository = seatRepository;
            _boardingPassRepository = boardingPassRepository;
            _flightService = flightService;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        public async Task<Passenger> GetPassengerByPassportNumber(string passportNumber)
        {
            try
            {
                _logger.LogInformation($"Attempting to find passenger with passport number: {passportNumber}");
                var passenger = await _passengerRepository.GetAsync(p => p.PassportNumber == passportNumber);
                
                if (passenger == null)
                {
                    _logger.LogWarning($"No passenger found with passport number: {passportNumber}");
                    return null;
                }

                _logger.LogInformation($"Found passenger: {passenger.Name}, Flight: {passenger.Flight?.FlightNumber}, Seat: {passenger.AssignedSeat?.SeatNumber}");
                return passenger;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while searching for passenger with passport number: {passportNumber}");
                throw; // Re-throw to let the controller handle it
            }
        }

        public async Task<BoardingPass> CheckInPassenger(int flightId, string passportNumber, string seatNumber)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                throw new Exception("Нислэг олдсонгүй");

            var passenger = await GetPassengerByPassportNumber(passportNumber);
            if (passenger == null)
                throw new Exception("Зорчигч олдсонгүй");

            var seat = await _seatRepository.GetAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);
            if (seat == null)
                throw new Exception("Суудал олдсонгүй");

            if (seat.IsOccupied)
                throw new Exception("Суудал захиалагдсан байна");

            // Суудал захиалах
            seat.IsOccupied = true;
            seat.Passenger = passenger;
            await _seatRepository.UpdateAsync(seat);

            // Бүртгэлийн хуудас үүсгэх
            var boardingPass = new BoardingPass
            {
                Flight = flight,
                Passenger = passenger,
                Seat = seat,
                CheckInTime = DateTime.Now,
                BoardingPassNumber = GenerateBoardingPassNumber(flight, passenger),
                BarcodeData = GenerateBarcodeData(flight, passenger, seat)
            };

            await _boardingPassRepository.AddAsync(boardingPass);
            await _notificationHub.NotifyCheckInCompleted(flightId, passportNumber);

            return boardingPass;
        }

        private string GenerateBoardingPassNumber(Flight flight, Passenger passenger)
        {
            // Format: FLTNO-PPTNO-TIMESTAMP
            return $"{flight.FlightNumber}-{passenger.PassportNumber}-{DateTime.Now:yyyyMMddHHmm}";
        }

        private string GenerateBarcodeData(Flight flight, Passenger passenger, Seat seat)
        {
            // Format: FLTNO|PPTNO|SEATNO|TIMESTAMP
            return $"{flight.FlightNumber}|{passenger.PassportNumber}|{seat.SeatNumber}|{DateTime.Now:yyyyMMddHHmm}";
        }

        public async Task<bool> GenerateBoardingPassAsync(string passportNumber, int flightId)
        {
            var passenger = await GetPassengerByPassportNumber(passportNumber);
            if (passenger == null) return false;

            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null) return false;

            var seat = await _seatRepository.GetAsync(s => s.FlightId == flightId && s.Passenger.PassportNumber == passportNumber);
            if (seat == null) return false;

            // Бүртгэлийн хуудас үүсгэх
            var boardingPass = new BoardingPass
            {
                Flight = flight,
                Passenger = passenger,
                Seat = seat,
                CheckInTime = DateTime.Now,
                BoardingPassNumber = GenerateBoardingPassNumber(flight, passenger),
                BarcodeData = GenerateBarcodeData(flight, passenger, seat)
            };

            await _boardingPassRepository.AddAsync(boardingPass);
            return true;
        }

        public async Task<Seat> GetPassengerSeatAsync(string passportNumber, int flightId)
        {
            return await _seatRepository.GetAsync(s => s.FlightId == flightId && s.Passenger.PassportNumber == passportNumber);
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