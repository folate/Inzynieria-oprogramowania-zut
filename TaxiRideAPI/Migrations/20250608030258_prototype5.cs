using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiRideAPI.Migrations
{
    /// <inheritdoc />
    public partial class prototype5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory");

            migrationBuilder.AlterColumn<int>(
                name: "RiderId",
                table: "RideHistory",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory");

            migrationBuilder.AlterColumn<int>(
                name: "RiderId",
                table: "RideHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RideHistory_Rides_RiderId",
                table: "RideHistory",
                column: "RiderId",
                principalTable: "Rides",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
