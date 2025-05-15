using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Airport.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardingPasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PassengerName = table.Column<string>(type: "TEXT", nullable: false),
                    PassportNumber = table.Column<string>(type: "TEXT", nullable: false),
                    FlightNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Destination = table.Column<string>(type: "TEXT", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gate = table.Column<string>(type: "TEXT", nullable: false),
                    SeatNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BoardingGroup = table.Column<string>(type: "TEXT", nullable: false),
                    BarCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardingPasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Destination = table.Column<string>(type: "TEXT", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gate = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSeats = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeatNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IsOccupied = table.Column<bool>(type: "INTEGER", nullable: false),
                    FlightId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PassportNumber = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nationality = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passengers_Seats_Id",
                        column: x => x.Id,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "Id", "DepartureTime", "Destination", "FlightNumber", "Gate", "Status", "TotalSeats" },
                values: new object[] { 1, new DateTime(2024, 5, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "Улаанбаатар", "MN123", "A1", 0, 180 });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "Id", "FlightId", "IsOccupied", "SeatNumber" },
                values: new object[,]
                {
                    { 1, 1, false, "1A" },
                    { 2, 1, false, "1B" },
                    { 3, 1, false, "1C" },
                    { 4, 1, false, "1D" },
                    { 5, 1, false, "1E" },
                    { 6, 1, false, "1F" },
                    { 7, 1, false, "2A" },
                    { 8, 1, false, "2B" },
                    { 9, 1, false, "2C" },
                    { 10, 1, false, "2D" },
                    { 11, 1, false, "2E" },
                    { 12, 1, false, "2F" },
                    { 13, 1, false, "3A" },
                    { 14, 1, false, "3B" },
                    { 15, 1, false, "3C" },
                    { 16, 1, false, "3D" },
                    { 17, 1, false, "3E" },
                    { 18, 1, false, "3F" },
                    { 19, 1, false, "4A" },
                    { 20, 1, false, "4B" },
                    { 21, 1, false, "4C" },
                    { 22, 1, false, "4D" },
                    { 23, 1, false, "4E" },
                    { 24, 1, false, "4F" },
                    { 25, 1, false, "5A" },
                    { 26, 1, false, "5B" },
                    { 27, 1, false, "5C" },
                    { 28, 1, false, "5D" },
                    { 29, 1, false, "5E" },
                    { 30, 1, false, "5F" },
                    { 31, 1, false, "6A" },
                    { 32, 1, false, "6B" },
                    { 33, 1, false, "6C" },
                    { 34, 1, false, "6D" },
                    { 35, 1, false, "6E" },
                    { 36, 1, false, "6F" },
                    { 37, 1, false, "7A" },
                    { 38, 1, false, "7B" },
                    { 39, 1, false, "7C" },
                    { 40, 1, false, "7D" },
                    { 41, 1, false, "7E" },
                    { 42, 1, false, "7F" },
                    { 43, 1, false, "8A" },
                    { 44, 1, false, "8B" },
                    { 45, 1, false, "8C" },
                    { 46, 1, false, "8D" },
                    { 47, 1, false, "8E" },
                    { 48, 1, false, "8F" },
                    { 49, 1, false, "9A" },
                    { 50, 1, false, "9B" },
                    { 51, 1, false, "9C" },
                    { 52, 1, false, "9D" },
                    { 53, 1, false, "9E" },
                    { 54, 1, false, "9F" },
                    { 55, 1, false, "10A" },
                    { 56, 1, false, "10B" },
                    { 57, 1, false, "10C" },
                    { 58, 1, false, "10D" },
                    { 59, 1, false, "10E" },
                    { 60, 1, false, "10F" },
                    { 61, 1, false, "11A" },
                    { 62, 1, false, "11B" },
                    { 63, 1, false, "11C" },
                    { 64, 1, false, "11D" },
                    { 65, 1, false, "11E" },
                    { 66, 1, false, "11F" },
                    { 67, 1, false, "12A" },
                    { 68, 1, false, "12B" },
                    { 69, 1, false, "12C" },
                    { 70, 1, false, "12D" },
                    { 71, 1, false, "12E" },
                    { 72, 1, false, "12F" },
                    { 73, 1, false, "13A" },
                    { 74, 1, false, "13B" },
                    { 75, 1, false, "13C" },
                    { 76, 1, false, "13D" },
                    { 77, 1, false, "13E" },
                    { 78, 1, false, "13F" },
                    { 79, 1, false, "14A" },
                    { 80, 1, false, "14B" },
                    { 81, 1, false, "14C" },
                    { 82, 1, false, "14D" },
                    { 83, 1, false, "14E" },
                    { 84, 1, false, "14F" },
                    { 85, 1, false, "15A" },
                    { 86, 1, false, "15B" },
                    { 87, 1, false, "15C" },
                    { 88, 1, false, "15D" },
                    { 89, 1, false, "15E" },
                    { 90, 1, false, "15F" },
                    { 91, 1, false, "16A" },
                    { 92, 1, false, "16B" },
                    { 93, 1, false, "16C" },
                    { 94, 1, false, "16D" },
                    { 95, 1, false, "16E" },
                    { 96, 1, false, "16F" },
                    { 97, 1, false, "17A" },
                    { 98, 1, false, "17B" },
                    { 99, 1, false, "17C" },
                    { 100, 1, false, "17D" },
                    { 101, 1, false, "17E" },
                    { 102, 1, false, "17F" },
                    { 103, 1, false, "18A" },
                    { 104, 1, false, "18B" },
                    { 105, 1, false, "18C" },
                    { 106, 1, false, "18D" },
                    { 107, 1, false, "18E" },
                    { 108, 1, false, "18F" },
                    { 109, 1, false, "19A" },
                    { 110, 1, false, "19B" },
                    { 111, 1, false, "19C" },
                    { 112, 1, false, "19D" },
                    { 113, 1, false, "19E" },
                    { 114, 1, false, "19F" },
                    { 115, 1, false, "20A" },
                    { 116, 1, false, "20B" },
                    { 117, 1, false, "20C" },
                    { 118, 1, false, "20D" },
                    { 119, 1, false, "20E" },
                    { 120, 1, false, "20F" },
                    { 121, 1, false, "21A" },
                    { 122, 1, false, "21B" },
                    { 123, 1, false, "21C" },
                    { 124, 1, false, "21D" },
                    { 125, 1, false, "21E" },
                    { 126, 1, false, "21F" },
                    { 127, 1, false, "22A" },
                    { 128, 1, false, "22B" },
                    { 129, 1, false, "22C" },
                    { 130, 1, false, "22D" },
                    { 131, 1, false, "22E" },
                    { 132, 1, false, "22F" },
                    { 133, 1, false, "23A" },
                    { 134, 1, false, "23B" },
                    { 135, 1, false, "23C" },
                    { 136, 1, false, "23D" },
                    { 137, 1, false, "23E" },
                    { 138, 1, false, "23F" },
                    { 139, 1, false, "24A" },
                    { 140, 1, false, "24B" },
                    { 141, 1, false, "24C" },
                    { 142, 1, false, "24D" },
                    { 143, 1, false, "24E" },
                    { 144, 1, false, "24F" },
                    { 145, 1, false, "25A" },
                    { 146, 1, false, "25B" },
                    { 147, 1, false, "25C" },
                    { 148, 1, false, "25D" },
                    { 149, 1, false, "25E" },
                    { 150, 1, false, "25F" },
                    { 151, 1, false, "26A" },
                    { 152, 1, false, "26B" },
                    { 153, 1, false, "26C" },
                    { 154, 1, false, "26D" },
                    { 155, 1, false, "26E" },
                    { 156, 1, false, "26F" },
                    { 157, 1, false, "27A" },
                    { 158, 1, false, "27B" },
                    { 159, 1, false, "27C" },
                    { 160, 1, false, "27D" },
                    { 161, 1, false, "27E" },
                    { 162, 1, false, "27F" },
                    { 163, 1, false, "28A" },
                    { 164, 1, false, "28B" },
                    { 165, 1, false, "28C" },
                    { 166, 1, false, "28D" },
                    { 167, 1, false, "28E" },
                    { 168, 1, false, "28F" },
                    { 169, 1, false, "29A" },
                    { 170, 1, false, "29B" },
                    { 171, 1, false, "29C" },
                    { 172, 1, false, "29D" },
                    { 173, 1, false, "29E" },
                    { 174, 1, false, "29F" },
                    { 175, 1, false, "30A" },
                    { 176, 1, false, "30B" },
                    { 177, 1, false, "30C" },
                    { 178, 1, false, "30D" },
                    { 179, 1, false, "30E" },
                    { 180, 1, false, "30F" }
                });

            migrationBuilder.InsertData(
                table: "Passengers",
                columns: new[] { "Id", "DateOfBirth", "FirstName", "LastName", "Nationality", "PassportNumber" },
                values: new object[] { 1, new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Бат", "Болд", "Монгол", "AA12345" });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_FlightId",
                table: "Seats",
                column: "FlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardingPasses");

            migrationBuilder.DropTable(
                name: "Passengers");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
