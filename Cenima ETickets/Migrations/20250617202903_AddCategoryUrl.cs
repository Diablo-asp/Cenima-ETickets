using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cenima_ETickets.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<string>(
                name: "CategoryUrl",
                table: "categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryUrl",
                table: "categories");

        }
    }
}
