using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Airport.Core.Interfaces;
using Airport.Core.Models;
using Microsoft.Extensions.Logging;

namespace Airport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;
        private readonly ILogger<PassengersController> _logger;

        public PassengersController(IPassengerService passengerService, ILogger<PassengersController> logger)
        {
            _passengerService = passengerService;
            _logger = logger;
        }

        [HttpGet("{passportNumber}")]
        public async Task<ActionResult<Passenger>> GetPassenger(string passportNumber)
        {
            try
            {
                _logger.LogInformation($"Searching for passenger with passport number: {passportNumber}");
                var passenger = await _passengerService.GetPassengerByPassportNumber(passportNumber);
                
                if (passenger == null)
                {
                    _logger.LogWarning($"Passenger not found with passport number: {passportNumber}");
                    return NotFound();
                }

                _logger.LogInformation($"Found passenger: {passenger.Name}, Flight: {passenger.Flight?.FlightNumber}, Seat: {passenger.AssignedSeat?.SeatNumber}");
                return Ok(passenger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while searching for passenger with passport number: {passportNumber}");
                return StatusCode(500, new { message = ex.Message, details = ex.ToString() });
            }
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