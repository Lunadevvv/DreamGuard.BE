using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizeProductModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAddonPrice",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductCustomizeDetails",
                table: "OrderItems",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductCustomizeDetails",
                table: "CartItems",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductCustomizeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCustomizeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VariantCustomizeTypes",
                columns: table => new
                {
                    CusId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverridePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantCustomizeTypes", x => new { x.CusId, x.ProductVariantId });
                    table.ForeignKey(
                        name: "FK_VariantCustomizeTypes_ProductCustomizeTypes_CusId",
                        column: x => x.CusId,
                        principalTable: "ProductCustomizeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VariantCustomizeTypes_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VariantCustomizeTypes_ProductVariantId",
                table: "VariantCustomizeTypes",
                column: "ProductVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariantCustomizeTypes");

            migrationBuilder.DropTable(
                name: "ProductCustomizeTypes");

            migrationBuilder.DropColumn(
                name: "TotalAddonPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductCustomizeDetails",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductCustomizeDetails",
                table: "CartItems");
        }
    }
}
