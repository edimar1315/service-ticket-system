using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceTicket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToUserIdToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "tickets",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "tickets");
        }
    }
}
