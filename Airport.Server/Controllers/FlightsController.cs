using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Airport.Core.Models;
using Airport.Core.Interfaces;

namespace Airport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

        [HttpGet("{flightNumber}")]
        public async Task<IActionResult> GetFlight(string flightNumber)
        {
            var flight = await _flightService.GetFlightByNumberAsync(flightNumber);
            if (flight == null)
                return NotFound();

            return Ok(flight);
        }

        [HttpPost("{flightId}/status")]
        public async Task<IActionResult> UpdateFlightStatus(int flightId, [FromBody] FlightStatus newStatus)
        {
            var success = await _flightService.UpdateFlightStatusAsync(flightId, newStatus);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpGet("{flightId}/seats")]
        public async Task<IActionResult> GetAvailableSeats(int flightId)
        {
            var seats = await _flightService.GetAvailableSeatsAsync(flightId);
            return Ok(seats);
        }

        [HttpPost("{flightId}/seats/{seatNumber}/assign")]
        public async Task<IActionResult> AssignSeat(int flightId, string seatNumber, [FromBody] string passportNumber)
        {
            var success = await _flightService.AssignSeatToPassengerAsync(flightId, seatNumber, passportNumber);
            if (!success)
                return BadRequest("Seat is not available or already assigned");

            return Ok();
        }

        [HttpGet("{flightId}/seats/{seatNumber}/availability")]
        public async Task<IActionResult> CheckSeatAvailability(int flightId, string seatNumber)
        {
            var isAvailable = await _flightService.IsSeatAvailableAsync(flightId, seatNumber);
            return Ok(new { IsAvailable = isAvailable });
        }
    }
} 