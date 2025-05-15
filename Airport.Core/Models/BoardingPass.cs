using System;
using System.ComponentModel.DataAnnotations;

namespace Airport.Core.Models
{
    public class BoardingPass
    {
        public int Id { get; set; }
        
        // Нислэгийн мэдээлэл
        [Required]
        public required Flight Flight { get; set; }
        
        // Зорчигчийн мэдээлэл
        [Required]
        public required Passenger Passenger { get; set; }
        
        // Суудлын мэдээлэл
        [Required]
        public required Seat Seat { get; set; }
        
        // Бүртгэлийн цаг
        public DateTime CheckInTime { get; set; }
        
        // Бүртгэлийн дугаар
        [Required]
        public required string BoardingPassNumber { get; set; }
        
        // QR код эсвэл баркодын мэдээлэл
        [Required]
        public required string BarcodeData { get; set; }

        public string GenerateBarCode()
        {
            // Format: FLTNO-PPTNO-SEATNO
            return $"{Flight.FlightNumber}-{Passenger.PassportNumber}-{Seat.SeatNumber}".Replace(" ", "");
        }
    }
} 