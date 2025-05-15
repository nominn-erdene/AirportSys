using System;

namespace Airport.Core.Models
{
    public class BoardingPass
    {
        public int Id { get; set; }
        public string PassengerName { get; set; }
        public string PassportNumber { get; set; }
        public string FlightNumber { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public string Gate { get; set; }
        public string SeatNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public string BoardingGroup { get; set; }
        public string BarCode { get; set; }

        public string GenerateBarCode()
        {
            // Format: FLTNO-PPTNO-SEATNO
            return $"{FlightNumber}-{PassportNumber}-{SeatNumber}".Replace(" ", "");
        }
    }
} 