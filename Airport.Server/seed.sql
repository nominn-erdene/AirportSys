-- Insert test flight
INSERT INTO Flights (FlightNumber, DepartureTime, Destination, Status, TotalSeats)
VALUES ('MN123', '2024-05-20 10:00:00', 'HKG', 0, 180);

-- Insert test passenger with passport number AA12345
INSERT INTO Passengers (Name, PassportNumber, Email, PhoneNumber, DateOfBirth, Nationality)
VALUES ('Test Passenger', 'AA12345', 'test@example.com', '+97699999999', '1990-01-01', 'MNG');

-- Insert test seats for the flight
INSERT INTO Seats (FlightId, SeatNumber, IsOccupied, Class)
VALUES (1, '1A', 0, 0),
       (1, '1B', 0, 0),
       (1, '1C', 0, 0); 