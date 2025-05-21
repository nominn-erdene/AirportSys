using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airport.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Baggages_Flights_FlightId",
                table: "Baggages");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Flights_FlightId",
                table: "Passengers");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Seats_Id",
                table: "Passengers");

            migrationBuilder.AddForeignKey(
                name: "FK_Baggages_Flights_FlightId",
                table: "Baggages",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Flights_FlightId",
                table: "Passengers",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Seats_Id",
                table: "Passengers",
                column: "Id",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Baggages_Flights_FlightId",
                table: "Baggages");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Flights_FlightId",
                table: "Passengers");

            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_Seats_Id",
                table: "Passengers");

            migrationBuilder.AddForeignKey(
                name: "FK_Baggages_Flights_FlightId",
                table: "Baggages",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Flights_FlightId",
                table: "Passengers",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_Seats_Id",
                table: "Passengers",
                column: "Id",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
