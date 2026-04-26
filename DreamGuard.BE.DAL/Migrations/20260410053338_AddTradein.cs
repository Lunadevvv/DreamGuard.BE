using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTradein : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Payments",
                newName: "PaymentType");

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTradeInEligible",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinTradeInPrice",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TradeInOrderId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTradeInUsed",
                table: "OrderItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TradeInOrders",
                columns: table => new
                {
                    TradeInOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    POrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    IsGood = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ReceiverName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    TradeInPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountToPay = table.Column<decimal>(type: "numeric", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeInOrders", x => x.TradeInOrderId);
                    table.ForeignKey(
                        name: "FK_TradeInOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeInOrders_OrderItems_POrderItemId",
                        column: x => x.POrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeInOrders_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeInOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_TradeInOrders_TradeInOrderId",
                        column: x => x.TradeInOrderId,
                        principalTable: "TradeInOrders",
                        principalColumn: "TradeInOrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeInImages",
                columns: table => new
                {
                    TradeInImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeInOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeInImages", x => x.TradeInImageId);
                    table.ForeignKey(
                        name: "FK_TradeInImages_TradeInOrders_TradeInOrderId",
                        column: x => x.TradeInOrderId,
                        principalTable: "TradeInOrders",
                        principalColumn: "TradeInOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SenderType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TradeInOrderId",
                table: "Payments",
                column: "TradeInOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CustomerId",
                table: "Conversations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_StaffId",
                table: "Conversations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_TradeInOrderId",
                table: "Conversations",
                column: "TradeInOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeInImages_TradeInOrderId",
                table: "TradeInImages",
                column: "TradeInOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeInOrders_CustomerId",
                table: "TradeInOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeInOrders_POrderItemId",
                table: "TradeInOrders",
                column: "POrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeInOrders_ProductVariantId",
                table: "TradeInOrders",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TradeInOrders_TradeInOrderId",
                table: "Payments",
                column: "TradeInOrderId",
                principalTable: "TradeInOrders",
                principalColumn: "TradeInOrderId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TradeInOrders_TradeInOrderId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "TradeInImages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "TradeInOrders");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TradeInOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsTradeInEligible",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinTradeInPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TradeInOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsTradeInUsed",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "Payments",
                newName: "Type");
        }
    }
}
