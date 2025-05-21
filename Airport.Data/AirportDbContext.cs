using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
        public DbSet<Baggage> Baggages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>()
                .HasMany(f => f.Seats)
                .WithOne(s => s.Flight)
                .HasForeignKey(s => s.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Passenger)
                .WithOne(p => p.AssignedSeat)
                .HasForeignKey<Passenger>(p => p.Id)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure relationship between Passenger and Flight
            modelBuilder.Entity<Passenger>()
                .HasOne(p => p.Flight)
                .WithMany()
                .HasForeignKey(p => p.FlightId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure required properties
            modelBuilder.Entity<Flight>()
                .Property(f => f.FlightNumber)
                .IsRequired();

            modelBuilder.Entity<Passenger>()
                .Property(p => p.PassportNumber)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.SeatNumber)
                .IsRequired();

            // Configure Baggage relationships
            modelBuilder.Entity<Baggage>()
                .HasOne(b => b.Passenger)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

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
                    Nationality = "Монгол",
                    FlightId = 1 // Assign the passenger to the sample flight
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

    public class AirportDbContextFactory : IDesignTimeDbContextFactory<AirportDbContext>
    {
        public AirportDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AirportDbContext>();
            optionsBuilder.UseSqlite("Data Source=Airport.db");

            return new AirportDbContext(optionsBuilder.Options);
        }
    }
} 