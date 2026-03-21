using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamGuard.BE.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ModifyServiceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_ServicePackageMappings_ServicePackageMappingId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_ServicePackageMappingId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "ServicePackageMappingId",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ServicePackages",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CustomNote",
                table: "ServiceOrders",
                newName: "CustomerNote");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ProductTypes",
                newName: "AddPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotalPrice",
                table: "ServiceOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "UserVoucherId",
                table: "ServiceOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceAssets",
                columns: table => new
                {
                    ServiceAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAssets", x => x.ServiceAssetId);
                    table.ForeignKey(
                        name: "FK_ServiceAssets_ServiceOrders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "ServiceOrders",
                        principalColumn: "SoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrderItems",
                columns: table => new
                {
                    ServiceOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageMappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrderItems", x => x.ServiceOrderItemId);
                    table.ForeignKey(
                        name: "FK_ServiceOrderItems_ServiceOrders_SoId",
                        column: x => x.SoId,
                        principalTable: "ServiceOrders",
                        principalColumn: "SoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceOrderItems_ServicePackageMappings_ServicePackageMapp~",
                        column: x => x.ServicePackageMappingId,
                        principalTable: "ServicePackageMappings",
                        principalColumn: "ServicePackageMappingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_UserVoucherId",
                table: "ServiceOrders",
                column: "UserVoucherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAssets_ServiceOrderId",
                table: "ServiceAssets",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderItems_ServicePackageMappingId",
                table: "ServiceOrderItems",
                column: "ServicePackageMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrderItems_SoId",
                table: "ServiceOrderItems",
                column: "SoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_UserVouchers_UserVoucherId",
                table: "ServiceOrders",
                column: "UserVoucherId",
                principalTable: "UserVouchers",
                principalColumn: "UserVoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_UserVouchers_UserVoucherId",
                table: "ServiceOrders");

            migrationBuilder.DropTable(
                name: "ServiceAssets");

            migrationBuilder.DropTable(
                name: "ServiceOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_UserVoucherId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "SubTotalPrice",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "UserVoucherId",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ServicePackages",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "CustomerNote",
                table: "ServiceOrders",
                newName: "CustomNote");

            migrationBuilder.RenameColumn(
                name: "AddPrice",
                table: "ProductTypes",
                newName: "Price");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ServicePackages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ServicePackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServicePackageMappingId",
                table: "ServiceOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServicePackageMappingId",
                table: "ServiceOrders",
                column: "ServicePackageMappingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_ServicePackageMappings_ServicePackageMappingId",
                table: "ServiceOrders",
                column: "ServicePackageMappingId",
                principalTable: "ServicePackageMappings",
                principalColumn: "ServicePackageMappingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
