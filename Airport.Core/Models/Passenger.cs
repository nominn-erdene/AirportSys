using System;

namespace Airport.Core.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        
        // Зорчигчийн овог
        public string? LastName { get; set; }
        
        // Зорчигчийн нэр
        public string? FirstName { get; set; }
        
        // Бүтэн нэр
        public string? Name 
        { 
            get => $"{LastName} {FirstName}";
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                
                var parts = value.Split(' ');
                if (parts.Length >= 2)
                {
                    LastName = parts[0];
                    FirstName = string.Join(" ", parts[1..]);
                }
            }
        }
        
        // Паспортын дугаар
        public string? PassportNumber { get; set; }
        
        // Утасны дугаар
        public string? PhoneNumber { get; set; }
        
        // И-мэйл хаяг
        public string? Email { get; set; }
        
        // Иргэншил
        public string? Nationality { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public virtual Seat? AssignedSeat { get; set; }

        // Нислэгийн ID
        public int? FlightId { get; set; }
        
        // Нислэгийн мэдээлэл
        public virtual Flight? Flight { get; set; }
    }
} 