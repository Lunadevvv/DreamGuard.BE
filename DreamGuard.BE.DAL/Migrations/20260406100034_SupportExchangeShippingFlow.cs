using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SupportExchangeShippingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShippingTasks_OrderId",
                table: "ShippingTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ShippingTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTasks_OrderId",
                table: "ShippingTasks",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShippingTasks_OrderId",
                table: "ShippingTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ShippingTasks");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTasks_OrderId",
                table: "ShippingTasks",
                column: "OrderId",
                unique: true);
        }
    }
}
