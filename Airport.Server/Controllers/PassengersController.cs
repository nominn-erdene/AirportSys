using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Airport.Core.Interfaces;

namespace Airport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;

        public PassengersController(IPassengerService passengerService)
        {
            _passengerService = passengerService;
        }

        [HttpGet("{passportNumber}")]
        public async Task<IActionResult> GetPassenger(string passportNumber)
        {
            var passenger = await _passengerService.GetPassengerByPassportAsync(passportNumber);
            if (passenger == null)
                return NotFound();

            return Ok(passenger);
        }

        [HttpPost("{passportNumber}/checkin")]
        public async Task<IActionResult> CheckIn(string passportNumber, [FromBody] CheckInRequest request)
        {
            var success = await _passengerService.CheckInPassengerAsync(passportNumber, request.FlightId, request.SeatNumber);
            if (!success)
                return BadRequest("Check-in failed");

            return Ok();
        }

        [HttpPost("{passportNumber}/boardingpass")]
        public async Task<IActionResult> GenerateBoardingPass(string passportNumber, [FromQuery] int flightId)
        {
            var success = await _passengerService.GenerateBoardingPassAsync(passportNumber, flightId);
            if (!success)
                return BadRequest("Could not generate boarding pass");

            return Ok();
        }

        [HttpGet("{passportNumber}/seat")]
        public async Task<IActionResult> GetPassengerSeat(string passportNumber, [FromQuery] int flightId)
        {
            var seat = await _passengerService.GetPassengerSeatAsync(passportNumber, flightId);
            if (seat == null)
                return NotFound();

            return Ok(seat);
        }
    }

    public class CheckInRequest
    {
        public int FlightId { get; set; }
        public string SeatNumber { get; set; }
    }
} 