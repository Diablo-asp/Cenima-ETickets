using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema_ETickets.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketsQuantityToMovieModle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketsQuantity",
                table: "movies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketsQuantity",
                table: "movies");
        }
    }
}
