using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDamageItemAndExchangeRequestedQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DamagedItems",
                table: "ShippingTasks",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExchangeRequestedQuantity",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamagedItems",
                table: "ShippingTasks");

            migrationBuilder.DropColumn(
                name: "ExchangeRequestedQuantity",
                table: "OrderItems");
        }
    }
}
