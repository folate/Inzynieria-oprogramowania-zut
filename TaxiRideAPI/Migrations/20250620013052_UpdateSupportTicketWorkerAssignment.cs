using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiRideAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportTicketWorkerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Users_AssignedToUserId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "SupportTickets",
                newName: "AssignedToWorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedToUserId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedToWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Workers_AssignedToWorkerId",
                table: "SupportTickets",
                column: "AssignedToWorkerId",
                principalTable: "Workers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Workers_AssignedToWorkerId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedToWorkerId",
                table: "SupportTickets",
                newName: "AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedToWorkerId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Users_AssignedToUserId",
                table: "SupportTickets",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
