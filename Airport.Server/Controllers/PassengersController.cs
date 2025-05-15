using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Airport.Core.Interfaces;
using Airport.Core.Models;

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
        public async Task<ActionResult<Passenger>> GetPassenger(string passportNumber)
        {
            var passenger = await _passengerService.GetPassengerByPassportNumber(passportNumber);
            if (passenger == null)
                return NotFound();

            return Ok(passenger);
        }

        [HttpPost("checkin")]
        public async Task<ActionResult<BoardingPass>> CheckInPassenger([FromBody] CheckInRequest request)
        {
            try
            {
                var boardingPass = await _passengerService.CheckInPassenger(
                    request.FlightId,
                    request.PassportNumber,
                    request.SeatNumber);

                return Ok(boardingPass);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{passportNumber}/seat/{flightId}")]
        public async Task<ActionResult<Seat>> GetPassengerSeat(string passportNumber, int flightId)
        {
            var seat = await _passengerService.GetPassengerSeatAsync(passportNumber, flightId);
            if (seat == null)
                return NotFound();

            return Ok(seat);
        }

        [HttpPost("{passportNumber}/boardingpass/{flightId}")]
        public async Task<ActionResult<bool>> GenerateBoardingPass(string passportNumber, int flightId)
        {
            var result = await _passengerService.GenerateBoardingPassAsync(passportNumber, flightId);
            if (!result)
                return BadRequest();

            return Ok(true);
        }
    }

    public class CheckInRequest
    {
        public int FlightId { get; set; }
        public string PassportNumber { get; set; }
        public string SeatNumber { get; set; }
    }
} 