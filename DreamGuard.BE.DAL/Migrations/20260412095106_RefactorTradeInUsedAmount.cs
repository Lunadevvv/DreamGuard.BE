using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTradeInUsedAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTradeInUsed",
                table: "OrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "ShippingTasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TradeInOrderId",
                table: "ShippingTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TradeInUsedAmount",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTasks_TradeInOrderId",
                table: "ShippingTasks",
                column: "TradeInOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingTasks_TradeInOrders_TradeInOrderId",
                table: "ShippingTasks",
                column: "TradeInOrderId",
                principalTable: "TradeInOrders",
                principalColumn: "TradeInOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingTasks_TradeInOrders_TradeInOrderId",
                table: "ShippingTasks");

            migrationBuilder.DropIndex(
                name: "IX_ShippingTasks_TradeInOrderId",
                table: "ShippingTasks");

            migrationBuilder.DropColumn(
                name: "TradeInOrderId",
                table: "ShippingTasks");

            migrationBuilder.DropColumn(
                name: "TradeInUsedAmount",
                table: "OrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "ShippingTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTradeInUsed",
                table: "OrderItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
