using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaxiRideAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverAvailabilities_Rides_RiderId",
                table: "DriverAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocuments_Rides_RiderId",
                table: "DriverDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_RideOffers_Rides_RiderId",
                table: "RideOffers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rides",
                table: "Rides");

            migrationBuilder.RenameTable(
                name: "Rides",
                newName: "Riders");

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastLocationUpdate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "registrationDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "CommissionRate",
                table: "TaxiCompanies",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaxiCompanies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "RideOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DriverRating",
                table: "RideOffers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "RideOffers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "RideHistory",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DropoffLatitude",
                table: "RideHistory",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DropoffLongitude",
                table: "RideHistory",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDuration",
                table: "RideHistory",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLatitude",
                table: "RideHistory",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLongitude",
                table: "RideHistory",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isVerified",
                table: "Riders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastActiveTime",
                table: "Riders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "licenseExpiryDate",
                table: "Riders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "licenseNumber",
                table: "Riders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "vehicleInfo",
                table: "Riders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Riders",
                table: "Riders",
                column: "id");

            migrationBuilder.CreateTable(
                name: "ComparisonRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PickupLocation = table.Column<string>(type: "text", nullable: false),
                    DropoffLocation = table.Column<string>(type: "text", nullable: false),
                    RequestTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VehicleTypeFilter = table.Column<string>(type: "text", nullable: true),
                    SortBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalRevenue = table.Column<double>(type: "double precision", nullable: false),
                    TotalCommission = table.Column<double>(type: "double precision", nullable: false),
                    TotalVat = table.Column<double>(type: "double precision", nullable: false),
                    TotalRides = table.Column<int>(type: "integer", nullable: false),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RideReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PickupLocation = table.Column<string>(type: "text", nullable: false),
                    DropoffLocation = table.Column<string>(type: "text", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedRiderId = table.Column<int>(type: "integer", nullable: true),
                    EstimatedPrice = table.Column<double>(type: "double precision", nullable: false),
                    VehicleTypePreference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideReservations_Riders_AssignedRiderId",
                        column: x => x.AssignedRiderId,
                        principalTable: "Riders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_RideReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketNumber = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RideId = table.Column<int>(type: "integer", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "integer", nullable: true),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    RefundAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_RideHistory_RideId",
                        column: x => x.RideId,
                        principalTable: "RideHistory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RiderId = table.Column<int>(type: "integer", nullable: false),
                    Make = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<string>(type: "text", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Riders_RiderId",
                        column: x => x.RiderId,
                        principalTable: "Riders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFromSupport = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonRequests_UserId",
                table: "ComparisonRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RideReservations_AssignedRiderId",
                table: "RideReservations",
                column: "AssignedRiderId");

            migrationBuilder.CreateIndex(
                name: "IX_RideReservations_UserId",
                table: "RideReservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedToUserId",
                table: "SupportTickets",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_RideId",
                table: "SupportTickets",
                column: "RideId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_SenderId",
                table: "TicketMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_RiderId",
                table: "Vehicles",
                column: "RiderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverAvailabilities_Riders_RiderId",
                table: "DriverAvailabilities",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocuments_Riders_RiderId",
                table: "DriverDocuments",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RideHistory_Riders_RiderId",
                table: "RideHistory",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_RideOffers_Riders_RiderId",
                table: "RideOffers",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverAvailabilities_Riders_RiderId",
                table: "DriverAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocuments_Riders_RiderId",
                table: "DriverDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_RideHistory_Riders_RiderId",
                table: "RideHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_RideOffers_Riders_RiderId",
                table: "RideOffers");

            migrationBuilder.DropTable(
                name: "ComparisonRequests");

            migrationBuilder.DropTable(
                name: "FinancialReports");

            migrationBuilder.DropTable(
                name: "RideReservations");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Riders",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "lastLocationUpdate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "registrationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "TaxiCompanies");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaxiCompanies");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "RideOffers");

            migrationBuilder.DropColumn(
                name: "DriverRating",
                table: "RideOffers");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "RideOffers");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "DropoffLatitude",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "DropoffLongitude",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "RideHistory");

            migrationBuilder.DropColumn(
                name: "isVerified",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "lastActiveTime",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "licenseExpiryDate",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "licenseNumber",
                table: "Riders");

            migrationBuilder.DropColumn(
                name: "vehicleInfo",
                table: "Riders");

            migrationBuilder.RenameTable(
                name: "Riders",
                newName: "Rides");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rides",
                table: "Rides",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverAvailabilities_Rides_RiderId",
                table: "DriverAvailabilities",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocuments_Rides_RiderId",
                table: "DriverDocuments",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_RideOffers_Rides_RiderId",
                table: "RideOffers",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
