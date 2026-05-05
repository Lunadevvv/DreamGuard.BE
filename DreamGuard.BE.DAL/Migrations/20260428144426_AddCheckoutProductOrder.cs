using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutProductOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutProductOrderId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutProductOrderId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CheckoutProductOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckoutOrderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAddonPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShippingFee = table.Column<decimal>(type: "numeric", nullable: false),
                    UserVoucherId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutProductOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckoutProductOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckoutProductOrders_UserVouchers_UserVoucherId",
                        column: x => x.UserVoucherId,
                        principalTable: "UserVouchers",
                        principalColumn: "UserVoucherId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CheckoutProductOrderId",
                table: "Payments",
                column: "CheckoutProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CheckoutProductOrderId",
                table: "Orders",
                column: "CheckoutProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutProductOrders_CustomerId",
                table: "CheckoutProductOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutProductOrders_UserVoucherId",
                table: "CheckoutProductOrders",
                column: "UserVoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CheckoutProductOrders_CheckoutProductOrderId",
                table: "Orders",
                column: "CheckoutProductOrderId",
                principalTable: "CheckoutProductOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CheckoutProductOrders_CheckoutProductOrderId",
                table: "Payments",
                column: "CheckoutProductOrderId",
                principalTable: "CheckoutProductOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CheckoutProductOrders_CheckoutProductOrderId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CheckoutProductOrders_CheckoutProductOrderId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "CheckoutProductOrders");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CheckoutProductOrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CheckoutProductOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CheckoutProductOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CheckoutProductOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Orders");
        }
    }
}
