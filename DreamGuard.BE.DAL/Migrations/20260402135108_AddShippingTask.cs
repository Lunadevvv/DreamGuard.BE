using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Payments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DefectQuantity",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ShippingTasks",
                columns: table => new
                {
                    ShippingTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StaffNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingTasks", x => x.ShippingTaskId);
                    table.ForeignKey(
                        name: "FK_ShippingTasks_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShippingTasks_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShippingEvidences",
                columns: table => new
                {
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShippingTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "text", nullable: false),
                    EvidenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingEvidences", x => x.EvidenceId);
                    table.ForeignKey(
                        name: "FK_ShippingEvidences_ShippingTasks_ShippingTaskId",
                        column: x => x.ShippingTaskId,
                        principalTable: "ShippingTasks",
                        principalColumn: "ShippingTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingEvidences_ShippingTaskId",
                table: "ShippingEvidences",
                column: "ShippingTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTasks_OrderId",
                table: "ShippingTasks",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTasks_StaffId",
                table: "ShippingTasks",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingEvidences");

            migrationBuilder.DropTable(
                name: "ShippingTasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DefectQuantity",
                table: "Inventories");
        }
    }
}
