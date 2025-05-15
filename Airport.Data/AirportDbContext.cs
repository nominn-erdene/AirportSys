using Microsoft.EntityFrameworkCore;
using Airport.Core.Models;
using System;
using System.Collections.Generic;

namespace Airport.Data
{
    public class AirportDbContext : DbContext
    {
        public AirportDbContext(DbContextOptions<AirportDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<BoardingPass> BoardingPasses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>()
                .HasMany(f => f.Seats)
                .WithOne(s => s.Flight)
                .HasForeignKey(s => s.FlightId);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Passenger)
                .WithOne(p => p.AssignedSeat)
                .HasForeignKey<Passenger>();

            modelBuilder.Entity<Flight>()
                .Property(f => f.FlightNumber)
                .IsRequired();

            modelBuilder.Entity<Passenger>()
                .Property(p => p.PassportNumber)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.SeatNumber)
                .IsRequired();

            // Sample data
            modelBuilder.Entity<Flight>().HasData(
                new Flight
                {
                    Id = 1,
                    FlightNumber = "MN123",
                    Destination = "Улаанбаатар",
                    DepartureTime = new DateTime(2024, 5, 15, 10, 0, 0),
                    Gate = "A1",
                    Status = FlightStatus.CheckingIn,
                    TotalSeats = 180
                }
            );

            modelBuilder.Entity<Passenger>().HasData(
                new Passenger
                {
                    Id = 1,
                    PassportNumber = "AA12345",
                    FirstName = "Бат",
                    LastName = "Болд",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Nationality = "Монгол"
                }
            );

            // Generate seats for the flight
            var seats = new List<Seat>();
            for (int row = 1; row <= 30; row++)
            {
                for (char col = 'A'; col <= 'F'; col++)
                {
                    seats.Add(new Seat
                    {
                        Id = ((row - 1) * 6) + (col - 'A' + 1),
                        FlightId = 1,
                        SeatNumber = $"{row}{col}",
                        IsOccupied = false
                    });
                }
            }
            modelBuilder.Entity<Seat>().HasData(seats);
        }
    }
} 