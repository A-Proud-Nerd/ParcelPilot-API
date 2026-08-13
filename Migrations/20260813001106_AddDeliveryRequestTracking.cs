using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelPilot.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryRequestTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "Deliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedPilotId",
                table: "Deliveries",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "RequestedPilotId",
                table: "Deliveries");
        }
    }
}
