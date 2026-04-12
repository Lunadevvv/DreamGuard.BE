using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationShipofTradeInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TradeInOrders_POrderItemId",
                table: "TradeInOrders");

            migrationBuilder.CreateIndex(
                name: "IX_TradeInOrders_POrderItemId",
                table: "TradeInOrders",
                column: "POrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TradeInOrders_POrderItemId",
                table: "TradeInOrders");

            migrationBuilder.CreateIndex(
                name: "IX_TradeInOrders_POrderItemId",
                table: "TradeInOrders",
                column: "POrderItemId",
                unique: true);
        }
    }
}
