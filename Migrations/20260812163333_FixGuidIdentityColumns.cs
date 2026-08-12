using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelPilot.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixGuidIdentityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "DeliveryStops");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "Pilots");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    RequiresSignature = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedPilotId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AgreedSubtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCharge = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pilots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    CompletedDeliveries = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityScore = table.Column<int>(type: "integer", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    VehiclePlate = table.Column<string>(type: "text", nullable: false),
                    ZonesJson = table.Column<string>(type: "text", nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false),
                    PerKmRate = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pilots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false),
                    AddressLine1 = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryStops_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PilotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PilotName = table.Column<string>(type: "text", nullable: false),
                    PilotRating = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DistanceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryStops_DeliveryId",
                table: "DeliveryStops",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_DeliveryId",
                table: "Quotes",
                column: "DeliveryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "DeliveryStops");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "Pilots");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessId = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    RequiresSignature = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedPilotId = table.Column<int>(type: "integer", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AgreedSubtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCharge = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pilots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    CompletedDeliveries = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityScore = table.Column<int>(type: "integer", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    VehiclePlate = table.Column<string>(type: "text", nullable: false),
                    ZonesJson = table.Column<string>(type: "text", nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false),
                    PerKmRate = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pilots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ProfileId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false),
                    AddressLine1 = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryStops_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryId = table.Column<int>(type: "integer", nullable: false),
                    PilotId = table.Column<int>(type: "integer", nullable: false),
                    PilotName = table.Column<string>(type: "text", nullable: false),
                    PilotRating = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DistanceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryStops_DeliveryId",
                table: "DeliveryStops",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_DeliveryId",
                table: "Quotes",
                column: "DeliveryId");
        }
    }
}
