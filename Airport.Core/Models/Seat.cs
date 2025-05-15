namespace Airport.Core.Models
{
    public class Seat
    {
        public int Id { get; set; }
        
        // Суудлын дугаар (жиш: A1, B2)
        public string SeatNumber { get; set; }
        
        // Суудлын төрөл (Economy, Business, First)
        public SeatClass Class { get; set; }
        
        // Суудал захиалагдсан эсэх
        public bool IsOccupied { get; set; }
        
        // Тухайн суудал дээр суух зорчигч
        public virtual Passenger? Passenger { get; set; }
        
        // Нислэгийн ID
        public int FlightId { get; set; }
        
        // Суудал аль нислэгт харьяалагдах
        public virtual Flight Flight { get; set; }
    }

    public enum SeatClass
    {
        Economy,
        Business,
        First
    }
} 