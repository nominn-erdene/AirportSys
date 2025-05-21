using System;
using System.ComponentModel.DataAnnotations;

namespace Airport.Core.Models
{
    public class Baggage
    {
        public int Id { get; set; }
        
        // Зорчигчийн мэдээлэл
        [Required]
        public required Passenger Passenger { get; set; }
        
        // Нислэгийн мэдээлэл
        [Required]
        public required Flight Flight { get; set; }
        
        // Ачааны жин (кг)
        public double Weight { get; set; }
        
        // Ачааны баркод
        [Required]
        public required string BarcodeNumber { get; set; }
        
        // Бүртгэсэн огноо
        public DateTime CheckInTime { get; set; }

        public string GenerateBarCode()
        {
            // Format: FLTNO-PPTNO-BAG-TIMESTAMP
            return $"{Flight.FlightNumber}-{Passenger.PassportNumber}-BAG-{CheckInTime:yyyyMMddHHmmss}".Replace(" ", "");
        }
    }
} 