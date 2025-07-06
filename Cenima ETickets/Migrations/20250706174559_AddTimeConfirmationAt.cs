using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema_ETickets.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeConfirmationAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationSentAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationSentAt",
                table: "AspNetUsers");
        }
    }
}
