using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FruitHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameQuantityCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quentity",
                table: "OrderItems",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Quentity",
                table: "CartItems",
                newName: "Quantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderItems",
                newName: "Quentity");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "CartItems",
                newName: "Quentity");
        }
    }
}
