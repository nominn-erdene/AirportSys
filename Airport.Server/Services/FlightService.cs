using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Airport.Core.Models;
using Airport.Core.Interfaces;
using Airport.Data.Interfaces;

namespace Airport.Server.Services
{
    public class FlightService : IFlightService
    {
        private readonly IRepository<Flight> _flightRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<Passenger> _passengerRepository;
        private readonly IFlightNotificationHub _notificationHub;

        public FlightService(
            IRepository<Flight> flightRepository,
            IRepository<Seat> seatRepository,
            IRepository<Passenger> passengerRepository,
            IFlightNotificationHub notificationHub)
        {
            _flightRepository = flightRepository;
            _seatRepository = seatRepository;
            _passengerRepository = passengerRepository;
            _notificationHub = notificationHub;
        }

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            return await _flightRepository.GetAllAsync();
        }

        public async Task<Flight> GetFlightByIdAsync(int id)
        {
            return await _flightRepository.GetByIdAsync(id);
        }

        public async Task<Flight> GetFlightByNumberAsync(string flightNumber)
        {
            var flights = await _flightRepository.FindAsync(f => f.FlightNumber == flightNumber);
            return flights.FirstOrDefault();
        }

        public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null) return false;

            flight.Status = newStatus;
            await _flightRepository.UpdateAsync(flight);
            var success = await _flightRepository.SaveChangesAsync();

            if (success)
            {
                await _notificationHub.NotifyFlightStatusChanged(flightId, newStatus);
            }

            return success;
        }

        public async Task<bool> AssignSeatToPassengerAsync(int flightId, string seatNumber, string passportNumber)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null) return false;

            var seat = (await _seatRepository.FindAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber)).FirstOrDefault();
            if (seat == null || seat.IsOccupied) return false;

            var passenger = (await _passengerRepository.FindAsync(p => p.PassportNumber == passportNumber)).FirstOrDefault();
            if (passenger == null) return false;

            seat.IsOccupied = true;
            seat.Passenger = passenger;
            await _seatRepository.UpdateAsync(seat);
            var success = await _seatRepository.SaveChangesAsync();

            if (success)
            {
                await _notificationHub.NotifySeatAssigned(flightId, seatNumber);
            }

            return success;
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId)
        {
            var seats = await _seatRepository.FindAsync(s => s.FlightId == flightId && !s.IsOccupied);
            return seats;
        }

        public async Task<bool> IsSeatAvailableAsync(int flightId, string seatNumber)
        {
            var seat = (await _seatRepository.FindAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber)).FirstOrDefault();
            return seat != null && !seat.IsOccupied;
        }
    }
} 