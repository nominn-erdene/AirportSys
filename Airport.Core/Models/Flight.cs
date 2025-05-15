using System;
using System.Collections.Generic;

namespace Airport.Core.Models
{
    public enum FlightStatus
    {
        CheckingIn,      // Бүртгэж байна
        Boarding,        // Онгоцонд сууж байна
        Departed,        // Ниссэн
        Delayed,         // Хойшилсон
        Cancelled        // Цуцалсан
    }

    public class Flight
    {
        public Flight()
        {
            Seats = new List<Seat>();
        }

        public int Id { get; set; }
        public string? FlightNumber { get; set; }
        public string? Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public string? Gate { get; set; }
        public FlightStatus Status { get; set; }
        public int TotalSeats { get; set; }
        public virtual ICollection<Seat> Seats { get; set; }
    }
} 